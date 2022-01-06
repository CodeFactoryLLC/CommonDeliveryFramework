using System.Linq;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Data.Sql
{
public static class SqlModelExtensions
    {
        /// <summary>
        /// Refreshes the implementation of an application model from a source sql model.
        /// </summary>
        /// <param name="source">The CodeFactory actions API starting from.</param>
        /// <param name="sourceModel">The target APP model to look for being transformed in gRpc.</param>
        /// <param name="sqlModelProject">The project that hosts sql data models.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <returns>Returns the implemented application model class or null.</returns>
        public static async Task<CsClass> RefreshSqlModelAsync(this IVsActions source, CsClass sourceModel,VsProject sqlModelProject,VsProject appModelProject)
        {
            if (source == null) throw new CodeFactoryException("No VSActions was provided cannot refresh the sql model.");
            if (sourceModel == null) throw new CodeFactoryException("No SQL source model was provided cannot refresh the sql model.");

            if (sqlModelProject == null) throw new CodeFactoryException("Could not load the SQL model project cannot refresh the sql model.");

            var appModel = await source.RefreshAppModelFromSqlModelAsync(sourceModel,appModelProject,sqlModelProject);

            if(appModel == null) return null;

            var sqlModel = await sqlModelProject.FindCSharpSourceByFileNameAsync($"{sourceModel.Name}.Transform.cs");

            var result = sqlModel == null ? await source.NewSqlModelTransformAsync(sqlModelProject, sourceModel, appModel): await sqlModel.UpdateSqlModelTransformAsync(sqlModelProject,sourceModel,appModel);
            
            return result;
        }

        /// <summary>
        /// Implements a new app data model. 
        /// </summary>
        /// <param name="source">CodeFactory Actions that can be accessed.</param>
        /// <param name="sqlModelProject">The project that hosts sql data models.</param>
        /// <param name="appModel">The app model that will be transformed.</param>
        /// <param name="sourceModel">The source model to be implemented.</param>
        /// <returns>Returns once the model has been created.</returns>
        public static async Task<CsClass> NewSqlModelTransformAsync(this IVsActions source, VsProject sqlModelProject, CsClass sourceModel,CsClass appModel)
        {

            var targetProject = sqlModelProject;
            CsClass result = null;


            if (targetProject == null) return null;
            if (sourceModel == null) return null;
            if (appModel == null) return null;  

            string targetModelName = sourceModel.Name;

            var properties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!properties.Any()) return null;

            var modelContent = sourceModel.BuildSqlTransformContent(appModel);
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

            classFormatter.AppendCodeLine(0, "using System;");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            classFormatter.AppendCodeLine(0, "using System.Linq;");
            classFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            classFormatter.AppendCodeLine(0, "using System.ComponentModel.DataAnnotations;");
            classFormatter.AppendCodeLine(0, "using System.ComponentModel.DataAnnotations.Schema;");
            classFormatter.AppendCodeLine(0, "using Microsoft.EntityFrameworkCore;");
            classFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
            classFormatter.AppendCodeLine(0, "{");

            classFormatter.AppendCodeLine(1, "/// <summary>");
            classFormatter.AppendCodeLine(1, $"/// Application model class implementation for '{targetModelName}' />");
            classFormatter.AppendCodeLine(1, "/// </summary>");
            classFormatter.AppendCodeLine(1, $"public partial class {targetModelName}");
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
                var document = await currentFolder.AddDocumentAsync($"{targetModelName}.Transform.cs", classFormatter.ReturnSource());

                var modelSourceCode = await document.GetCSharpSourceModelAsync();

                result = modelSourceCode.Classes.FirstOrDefault();

            }
            else
            {
                var document = await targetProject.AddDocumentAsync($"{targetModelName}.Transform.cs", classFormatter.ReturnSource());

                var modelSourceCode = await document.GetCSharpSourceModelAsync();

                result = modelSourceCode.Classes.FirstOrDefault();
            }

            var updatedSourceModel = await sqlModelProject.FindCSharpSourceByClassNameAsync(sourceModel.Name);

            if (updatedSourceModel == null) throw new CodeFactoryException("Could not load the updated sql data model operation canceled.");

            result = updatedSourceModel.SourceCode.Classes.First();

            if (result == null) throw new CodeFactoryException("Could not load the updated sql data model operation canceled.");

            return result;
        }

        /// <summary>
        /// Updates an existing sql model transform definition from the source sql entity.
        /// </summary>
        /// <param name="sqlModelSource">Source sql transform model to update.</param>
        /// <param name="sqlModelProject">The project that hosts sql data models.</param>
        /// <param name="sourceModel">The source class from the sql entity to use for the update.</param>
        /// <param name="appModel">The target app model to be used in the transform.</param>
        /// <returns>Updated sql model class with updated transform methods.</returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task<CsClass> UpdateSqlModelTransformAsync(this VsCSharpSource sqlModelSource, VsProject sqlModelProject, CsClass sourceModel,CsClass appModel)
        {

            if (sqlModelSource == null) throw new CodeFactoryException("No sql model source was provided cannot complete operation.");

            if (sourceModel == null) throw new CodeFactoryException("No source model was provided cannot update sql model transform.");

            if (appModel == null) throw new CodeFactoryException("No app model was provided cannot update sql model transform.");

            var modelContent = sourceModel.BuildSqlTransformContent(appModel);

            if (modelContent == null) throw new CodeFactoryException($"Cannot load the updated transform methods for '{sourceModel.Name}', operation is cancelled.");

            CsClass sqlTransformModel = sqlModelSource.SourceCode.Classes.FirstOrDefault();

            if (sqlTransformModel == null) throw new CodeFactoryException("No app model class was found from the supplied model source operation canceled.");

            var modelSource = sqlModelSource.SourceCode;

            if (!modelSource.HasUsingStatement("System")) modelSource = await modelSource.AddUsingStatementAsync("System");

            if (!modelSource.HasUsingStatement("System.Collections.Generic")) modelSource = await modelSource.AddUsingStatementAsync("System.Collections.Generic");

            if (!modelSource.HasUsingStatement("System.Linq")) modelSource = await modelSource.AddUsingStatementAsync("System.Linq");

            if (!modelSource.HasUsingStatement("System.Threading.Tasks")) modelSource = await modelSource.AddUsingStatementAsync("System.Threading.Tasks");

            if (!modelSource.HasUsingStatement("System.ComponentModel.DataAnnotations")) modelSource = await modelSource.AddUsingStatementAsync("System.ComponentModel.DataAnnotations");

            if (!modelSource.HasUsingStatement("System.ComponentModel.DataAnnotations.Schema")) modelSource = await modelSource.AddUsingStatementAsync("System.ComponentModel.DataAnnotations.Schema");

            if (!modelSource.HasUsingStatement("Microsoft.EntityFrameworkCore")) modelSource = await modelSource.AddUsingStatementAsync("Microsoft.EntityFrameworkCore");


            var transformClass = modelSource.Classes.First();

            if (transformClass == null) throw new CodeFactoryException("Could not load app data model operation canceled.");

            var createSqlModelMethod = transformClass.Methods.FirstOrDefault(m => m.Name == "CreateSqlModel");

            if (createSqlModelMethod != null)
            {
                var removedSqlModelMethodSource = await createSqlModelMethod.DeleteAsync();

                transformClass = removedSqlModelMethodSource.Classes.First();
            }

            var createAppModelMethod = transformClass.Methods.FirstOrDefault(m => m.Name == "CreateAppModel");

            if (createAppModelMethod != null)
            { 
                var removedAppModelMethodSource = await createAppModelMethod.DeleteAsync();

                transformClass = removedAppModelMethodSource.Classes.First();
            }

            CsClass result = null;

            modelSource = await transformClass.AddToEndAsync(modelContent.ReturnSource());

            var updatedSourceModel = await sqlModelProject.FindCSharpSourceByClassNameAsync(sourceModel.Name);

            if (updatedSourceModel == null) throw new CodeFactoryException("Could not load the updated sql data model operation canceled.");

            result = updatedSourceModel.SourceCode.Classes.First();
            
            if (result == null) throw new CodeFactoryException("Could not load the updated sql data model operation canceled.");

            return result;
        }

        /// <summary>
        /// Builds out the list of properties to add to an app model from a SQL model.
        /// </summary>
        /// <param name="sourceModel">SQL model to extract properties from.</param>
        /// <param name="appModel">The application model to be used for building the transform.</param>
        /// <returns></returns>
        public static SourceFormatter BuildSqlTransformContent(this CsClass sourceModel, CsClass appModel)
        {
            if (sourceModel == null) return null;

            var publicProperties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!publicProperties.Any()) return null;

            var properties =  publicProperties.Where(p => p.IsSupportedAppModelProperty());

            if (!properties.Any()) return null;


            SourceFormatter classFormatter = new SourceFormatter();

            SourceFormatter createAppModelFormatter = new SourceFormatter();

            createAppModelFormatter.AppendCodeLine(2, "/// <summary>");
            createAppModelFormatter.AppendCodeLine(2, $"/// Creates an instance of the app model <see cref=\"{appModel.Namespace}.{appModel.Name}\"/> from a <see cref=\"{sourceModel.Name}\"/> model.");
            createAppModelFormatter.AppendCodeLine(2, "/// </summary>");
            createAppModelFormatter.AppendCodeLine(2, "/// <returns>Instance of the app model.</returns>");
            createAppModelFormatter.AppendCodeLine(2, $"public {appModel.Namespace}.{appModel.Name} CreateAppModel()");
            createAppModelFormatter.AppendCodeLine(2, "{");

            SourceFormatter createSqlModelFormatter = new SourceFormatter();

            createSqlModelFormatter.AppendCodeLine(2, "/// <summary>");
            createSqlModelFormatter.AppendCodeLine(2, $"/// Creates an instance of the sql Model <see cref=\"{sourceModel.Name}\"/> from a <see cref=\"{appModel.Namespace}.{appModel.Name}\"/> model.");
            createSqlModelFormatter.AppendCodeLine(2, "/// </summary>");
            createSqlModelFormatter.AppendCodeLine(2, "/// <param name=\"source\">The application model to transform.</param>");
            createSqlModelFormatter.AppendCodeLine(2, "/// <returns>Instance of the Sql model.</returns>");
            createSqlModelFormatter.AppendCodeLine(2, $"public static {sourceModel.Name} CreateSqlModel({appModel.Namespace}.{appModel.Name} source)");
            createSqlModelFormatter.AppendCodeLine(2, "{");

            bool firstParameter = true;

            foreach (var property in properties)
            {

                if (firstParameter)
                {
                    createSqlModelFormatter.AppendCodeLine(3, $"return new {sourceModel.Name} {{ {property.Name} = source.{property.Name}");
                    createAppModelFormatter.AppendCodeLine(3, $"return new {appModel.Namespace}.{appModel.Name} {{ {property.Name} = {property.DefaultValueCSharpSyntax()}");
                    firstParameter = false;
                }
                else
                {
                    createSqlModelFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name}");
                    createAppModelFormatter.AppendCodeLine(3, $", {property.Name} = {property.DefaultValueCSharpSyntax()}");
                }
            }

            createAppModelFormatter.AppendCode("};");
            createAppModelFormatter.AppendCodeLine(2, "}");
            createAppModelFormatter.AppendCodeLine(2);

            createSqlModelFormatter.AppendCode("};");
            createSqlModelFormatter.AppendCodeLine(2, "}");
            createSqlModelFormatter.AppendCodeLine(2);

            classFormatter.AppendCode(createSqlModelFormatter.ReturnSource());
            classFormatter.AppendCode(createAppModelFormatter.ReturnSource());

            return classFormatter;
        }

        
    }

}