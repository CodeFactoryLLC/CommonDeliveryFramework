using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Data.Sql
{
    /// <summary>
    /// Extensions class that handles the creation and management of application models.
    /// </summary>
    public static class AppModelExtensions
    {
        /// <summary>
        /// Refreshes the implementation of an application model from a source sql model.
        /// </summary>
        /// <param name="source">The CodeFactory actions API starting from.</param>
        /// <param name="sourceModel">The source sql model to refresh the application model.</param>
        /// <param name="appModelProject">The target project the application model will be implemented in.</param>
        /// <param name="sqlModelProject">The target project that holds the original definition of sql data models.</param>
        /// <returns>Returns the implemented application model class or null.</returns>
        public static async Task<CsClass> RefreshAppModelFromSqlModelAsync(this IVsActions source, CsClass sourceModel, VsProject appModelProject,VsProject sqlModelProject)
        {
            if (source == null) throw new CodeFactoryException("No VSActions was provided cannot refresh the app model from the sql model");
            if (sourceModel == null) throw new CodeFactoryException("No SQL source model was provided cannot refresh the app model from the sql model");

            if(appModelProject == null) throw new CodeFactoryException("Could not load the app model project, cannot refresh the app model from the sql model.");

            var appModel = await appModelProject.FindCSharpSourceByClassNameAsync(sourceModel.Name);

            CsClass result = appModel == null ? await source.NewAppModelAsync(appModelProject,sqlModelProject, sourceModel) : await appModel.UpdateAppModelAsync(sourceModel);

            return result;
        }

        /// <summary>
        /// Implements a new app data model. 
        /// </summary>
        /// <param name="source">CodeFactory Actions that can be accessed.</param>
        /// <param name="appModelProject">The app model project to be added to.</param>
        /// <param name="sqlModelProject">The model project for the sql entity being used to build the new app model came from.</param>
        /// <param name="sourceModel">The source model to be implemented.</param>
        /// <returns>Returns once the model has been created.</returns>
        public static async Task<CsClass> NewAppModelAsync(this IVsActions source, VsProject appModelProject, VsProject sqlModelProject, CsClass sourceModel)
        {

            var targetProject = appModelProject;
            CsClass result = null;


            if (appModelProject == null)
                throw new CodeFactoryException(
                    "No application model project was provided cannot create a new application model");
            if (sqlModelProject == null)
                throw new CodeFactoryException(
                    "No sql model project was provided cannot create a new application model.");

            if (sourceModel == null) return null;

            string targetModelName = sourceModel.Name;
         
            var properties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!properties.Any()) return null;

            var modelContent = sourceModel.BuildAppModelContent();

            if (modelContent == null) return null;

            var sourceModelCode = await sqlModelProject.FindSource(sourceModel);

            if (sourceModelCode == null)
            {
                throw new CodeFactoryException(
                    $"Could not load the source code file for the model '{sourceModel.Namespace}.{sourceModel.Name}' operation could not be completed.");
            }

            var folderStructure = await sourceModelCode.GetProjectFolderStructureAsync();

            var solution = await source.GetSolutionAsync();

            string targetNamespace = targetProject.CreateModelNamespace(folderStructure);

            if (targetNamespace == null) return null;

            SourceFormatter classFormatter = new SourceFormatter();

            NamespaceManager manager = new NamespaceManager(ImmutableList<CsUsingStatement>.Empty, targetNamespace);
            manager.AppendingNamespace("System");
            classFormatter.AppendCodeLine(0, "using System;");
            manager.AppendingNamespace("System.Collections.Generic");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manager.AppendingNamespace("System.Linq");
            classFormatter.AppendCodeLine(0, "using System.Linq;");
            manager.AppendingNamespace("System.Threading.Tasks");
            classFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manager.AppendingNamespace(appModelProject.DefaultNamespace);
            classFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
            classFormatter.AppendCodeLine(0, "{");

            classFormatter.AppendCodeLine(1, "/// <summary>");
            classFormatter.AppendCodeLine(1, $"/// Application model class implementation for '{targetModelName}' />");
            classFormatter.AppendCodeLine(1, "/// </summary>");
            classFormatter.AppendCodeLine(1, $"public class {targetModelName}");
            classFormatter.AppendCodeLine(1, "{");
            classFormatter.AppendCode(modelContent.ReturnSource());
            classFormatter.AppendCodeLine(1, "}");

            classFormatter.AppendCodeLine(0, "}");


            if (folderStructure.Any())
            {
                bool firstFolder = true;

                VsProjectFolder currentFolder = null;
                foreach (var folderName in folderStructure)
                {

                    if (firstFolder)
                    {
                        var children = await targetProject.GetChildrenAsync(false, false);

                        currentFolder = children.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>()
                            .FirstOrDefault(f => f.Name == folderName) ?? await targetProject.AddProjectFolderAsync(folderName);

                        firstFolder = false;
                    }
                    else
                    {
                        var folderChildren = await currentFolder.GetChildrenAsync(false, false);

                        var levelFolder = folderChildren.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>()
                            .FirstOrDefault(f => f.Name == folderName) ?? await currentFolder.AddProjectFolderAsync(folderName);

                        if (levelFolder == null)
                        {
                            throw new CodeFactoryException(
                                $"Cannot Create Model {targetModelName} was not able to create project folder '{folderName}'");
                        }
                    }
                }

                if (currentFolder == null)
                {
                    throw new CodeFactoryException(
                        $"Cannot Create Model {targetModelName} was not able to create project folders.");
                }
                var document = await currentFolder.AddDocumentAsync($"{targetModelName}.cs", classFormatter.ReturnSource());

                var modelSourceCode = await document.GetCSharpSourceModelAsync();

                result = modelSourceCode.Classes.FirstOrDefault();

            }
            else
            {
                var document = await targetProject.AddDocumentAsync($"{targetModelName}.cs", classFormatter.ReturnSource());

                var modelSourceCode = await document.GetCSharpSourceModelAsync();

                result = modelSourceCode.Classes.FirstOrDefault();
            }

            return result;

        }

        /// <summary>
        /// Updates an existing application model definition from the source sql entity.
        /// </summary>
        /// <param name="appModelSource">Source application model to update.</param>
        /// <param name="sourceModel">The source class from the sql entity to use for the update.</param>
        /// <returns>Updated app model class.</returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task<CsClass> UpdateAppModelAsync( this VsCSharpSource appModelSource, CsClass sourceModel)
        {

            if (appModelSource == null) throw new CodeFactoryException("No app model source was provided cannot complete operation.");

            if (sourceModel == null) throw new CodeFactoryException("No source model was provided cannot update Grpc model.");

            CsClass appModel = appModelSource.SourceCode.Classes.FirstOrDefault();

            if (appModel == null) throw new CodeFactoryException("No app model class was found from the supplied model source operation canceled.");

            var modelSource = appModelSource.SourceCode;

            if (!modelSource.HasUsingStatement("System")) modelSource = await modelSource.AddUsingStatementAsync("System");

            if (!modelSource.HasUsingStatement("System.Collections.Generic")) modelSource = await modelSource.AddUsingStatementAsync("System.Collections.Generic");

            if (!modelSource.HasUsingStatement("System.Linq")) modelSource = await modelSource.AddUsingStatementAsync("System.Linq");

            if (!modelSource.HasUsingStatement("System.Threading.Tasks")) modelSource = await modelSource.AddUsingStatementAsync("System.Threading.Tasks");

            appModel = modelSource.Classes.First();

            if (appModel == null) throw new CodeFactoryException("Could not load app data model operation canceled.");

            var properties =
                appModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public & !p.IsStatic);

            if (properties.Any())
            {

                foreach (var currentProperty in properties)
                {
                    var targetModel = appModel.GetModel(currentProperty.LookupPath);

                    if (targetModel == null) continue;

                    var targetProperty = targetModel as CsProperty;

                    if (targetProperty == null) continue;

                    modelSource = await targetProperty.DeleteAsync(appModel.SourceDocument);

                    appModel = modelSource.Classes.First();

                    if (appModel == null)
                        throw new CodeFactoryException("Could not load app data model operation canceled.");

                }

            }

            var modelContent = sourceModel.BuildAppModelContent();

            if (modelContent != null)
            {
                modelSource = await appModel.AddToEndAsync(modelContent.ReturnSource());

                appModel = modelSource.Classes.First();
            }
            if (appModel == null)
                throw new CodeFactoryException("Could not load app data model operation canceled.");


            return appModel;
        }



        /// <summary>
        /// Builds out the list of properties to add to an app model from a SQL model.
        /// </summary>
        /// <param name="sourceModel">SQL model to extract properties from.</param>
        /// <returns></returns>
        public static  SourceFormatter BuildAppModelContent(this CsClass sourceModel)
        {
            if (sourceModel == null) return null;

            var publicProperties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!publicProperties.Any()) return null;

            var properties = publicProperties.Where(IsSupportedAppModelProperty).ToList();

            if(!properties.Any()) return null;


            SourceFormatter classFormatter = new SourceFormatter();

            int propertyCount = 0;
            foreach (var property in properties)
            {
                propertyCount++;

                var propertyType = property.PropertyType;

                if (property.HasDocumentation)
                {
                    foreach (var documentation in property.CSharpFormatXmlDocumentationEnumerator())
                    {
                        //Appending each XML document line to the being of the member definition.
                        classFormatter.AppendCodeLine(2, documentation);
                    }
                }

                var attributes = property.HasAttributes ? property.Attributes.Where(IsSupportedAppModelAttribute).ToList() : new List<CsAttribute>();

                if (attributes.Any())
                    //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                    foreach (var attributeSyntax in attributes.CSharpFormatAttributeDeclarationEnumerator())
                    {
                        //Appending each attribute definition before the member definition.
                        classFormatter.AppendCodeLine(2, attributeSyntax);
                    }

                //Generating the standard property signature for C# property definition.
                var typeName = property?.PropertyType.GetPropertyTypeDefinitionSyntaxRemoveNullableFromDefinition();
                classFormatter.AppendCodeLine(2, property.FormatCSharpProperty(typeName));
                classFormatter.AppendCodeLine(2);
            }

            return  classFormatter;
        }

        /// <summary>
        /// Checks the attributes on a property supplied from a sql model to make sure its not an external table reference.
        /// </summary>
        /// <param name="source">Property to validate.</param>
        /// <returns>True if the property is supported for an app model, false if not.</returns>
        public static bool IsSupportedAppModelProperty(this CsProperty source)
        {
            if(source == null) return false;
            if (!source.HasAttributes) return true;

            var result = false;

            result =  !source.Attributes.Any(a =>
                {
                    var isNotAllowed = false;
                    var attributeType = a.Type;
                    if (attributeType.Name == "InversePropertyAttribute" & attributeType.Namespace == "System.ComponentModel.DataAnnotations.Schema") isNotAllowed = true;
                    if (attributeType.Name == "ForeignKeyAttribute" & attributeType.Namespace == "System.ComponentModel.DataAnnotations.Schema") isNotAllowed = true;
                    return isNotAllowed;
                });

            return result;
        }

        /// <summary>
        /// Checks the properties attribute to see if it supported on an app model.
        /// </summary>
        /// <param name="source">Attribute to validate.</param>
        /// <returns>True if the attribute is support, false if the attribute is not supported.</returns>
        private static bool IsSupportedAppModelAttribute(this CsAttribute source)
        {
            if (source == null) return false;

            var result = true;
            var attributeType = source.Type;

            if (attributeType == null) return result;
            if (attributeType.Name == "KeyAttribute" & attributeType.Namespace == "System.ComponentModel.DataAnnotations") result = false;
            if (attributeType.Name == "ColumnAttribute" & attributeType.Namespace == "System.ComponentModel.DataAnnotations.Schema") result = false;

            return result;
        }
    }
}