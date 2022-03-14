using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Service.Grpc
{
 /// <summary>
    /// C# extensions and supporting logic to manage Grpc Model Generation.
    /// </summary>
    public static partial class GrpcModelExtensions
    {

        /// <summary>
        /// Suffix to be appended to all gRpc models.
        /// </summary>
        public const string ServiceRpcSuffix = "Rpc";

        #region AppModel Refresh Based Logic

        /// <summary>
        /// Find a Grpc model from a target project
        /// </summary>
        /// <param name="source">The CodeFactory actions API starting from.</param>
        /// <param name="rpcModelProject">The target gRpc project to search.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The target APP model to look for being transformed in gRpc.</param>
        /// <param name="processedModels">List of models that all ready been processed, keeps from creating a runaway situation.</param>
        /// <returns>Returns the implemented gRpc class that support the source model, or null.</returns>
        public static async Task<ModelProcessResult<CsClass>> RefreshGrpcModelAsync(this IVsActions source, VsProject rpcModelProject,VsProject appModelProject, CsClass sourceModel, ImmutableList<ModelProcessed> processedModels = null)
        {
            if (source == null) return null;
            if (sourceModel == null) return null;

            var refreshedModels = processedModels ?? ImmutableList<ModelProcessed>.Empty;

            CsClass result = null;

            string targetModelName = $"{sourceModel.Name}{ServiceRpcSuffix}";

            var sourceCodeFile = await rpcModelProject.FindModel(sourceModel, targetModelName);

            var processResult = sourceCodeFile == null
                ? await source.ImplementNewGrpcModelAsync(rpcModelProject, appModelProject, sourceModel, targetModelName, refreshedModels)
                : await UpdateGrpcModelAsync(source, rpcModelProject,appModelProject, sourceCodeFile, sourceModel, refreshedModels);

            if (processResult?.ProcessedModel != null) refreshedModels = processedModels;

            result = processResult?.Result;

            return new ModelProcessResult<CsClass>(refreshedModels, result);
        }

        /// <summary>
        /// Implements a new data model with conversion. 
        /// </summary>
        /// <param name="source">CodeFactory Actions that can be accessed.</param>
        /// <param name="rpcModelProject">The gRpc project to be added to.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The source model to be implemented.</param>
        /// <param name="targetModelName">the name of the target model. </param>
        /// <param name="processedModels">List of models that all ready been processed, keeps from creating a runaway situation.</param>
        /// <returns>Returns once the model has been created.</returns>
        public static async Task<ModelProcessResult<CsClass>> ImplementNewGrpcModelAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject
            , CsClass sourceModel, string targetModelName, ImmutableList<ModelProcessed> processedModels)
        {

            var targetProject = rpcModelProject;
            CsClass result = null;

            var refreshedModels = processedModels;
            if (refreshedModels == null) return null;
            if (rpcModelProject == null) return null;
            if (sourceModel == null) return null;

            if (string.IsNullOrEmpty(targetModelName)) return null;

            var properties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!properties.Any()) return null;

            var sourceModelCode = await appModelProject.FindSource(sourceModel);

            if (sourceModelCode == null)
            {
                throw new CodeFactoryException(
                    $"Could not load the source code file for the model '{sourceModel.Namespace}.{sourceModel.Name}' operation could not be completed.");
            }

            var folderStructure = await sourceModelCode.GetProjectFolderStructureAsync();

            var solution = await source.GetSolutionAsync();

            string targetNamespace = rpcModelProject.CreateModelNamespace(folderStructure);

            if (targetNamespace == null) return null;

            SourceFormatter classFormatter = new SourceFormatter();


            SourceFormatter sourceCreateFormatter = new SourceFormatter();

            sourceCreateFormatter.AppendCodeLine(2, "/// <summary>");
            sourceCreateFormatter.AppendCodeLine(2, $"/// Creates an instance of the Grpc Model <see cref=\"{targetModelName}\"/> from a <see cref=\"{sourceModel.Name}\"/> model.");
            sourceCreateFormatter.AppendCodeLine(2, "/// </summary>");
            sourceCreateFormatter.AppendCodeLine(2, "/// <param name=\"source\">The model to transform into a Grpc Model.</param>");
            sourceCreateFormatter.AppendCodeLine(2, "/// <returns>Instance of the Grpc model.</returns>");
            sourceCreateFormatter.AppendCodeLine(2, $"public static {targetModelName} CreateGrpcModel({sourceModel.Name} source)");
            sourceCreateFormatter.AppendCodeLine(2, "{");


            SourceFormatter destinationCreateFormatter = new SourceFormatter();
            destinationCreateFormatter.AppendCodeLine(2, "/// <summary>");
            destinationCreateFormatter.AppendCodeLine(2, $"/// Creates an instance of the Model <see cref=\"{sourceModel.Name}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
            destinationCreateFormatter.AppendCodeLine(2, "/// </summary>");
            destinationCreateFormatter.AppendCodeLine(2, "// <returns>Instance of the model.</returns>");
            destinationCreateFormatter.AppendCodeLine(2, $"public {sourceModel.Name} CreateAppModel()");
            destinationCreateFormatter.AppendCodeLine(2, "{");

            bool sourceFirstParameter = true;
            StringBuilder sourceCreateStringBuilder = new StringBuilder();

            bool destinationFirstParameter = true;
            StringBuilder destinationCreateStringBuilder = new StringBuilder();



            NamespaceManager manager = new NamespaceManager(ImmutableList<CsUsingStatement>.Empty, targetNamespace);
            manager.AppendingNamespace("System");
            classFormatter.AppendCodeLine(0, "using System;");
            manager.AppendingNamespace("System.Collections.Generic");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manager.AppendingNamespace("System.Linq");
            classFormatter.AppendCodeLine(0, "using System.Linq;");
            manager.AppendingNamespace("System.Threading.Tasks");
            classFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manager.AppendingNamespace("CommonDeliveryFramework");
            classFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manager.AppendingNamespace("CommonDeliveryFramework.Grpc");
            classFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manager.AppendingNamespace(appModelProject.DefaultNamespace);
            classFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manager.AppendingNamespace("ProtoBuf");
            classFormatter.AppendCodeLine(0, "using ProtoBuf;");
            if (sourceModel.Namespace != appModelProject.DefaultNamespace)
            {
                manager.AppendingNamespace(sourceModel.Namespace);
                classFormatter.AppendCodeLine(0, $"using {sourceModel.Namespace};");
            }
            classFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");

            classFormatter.AppendCodeLine(0, "{");

            classFormatter.AppendCodeLine(1, "/// <summary>");
            classFormatter.AppendCodeLine(1, $"/// Grpc model class that represents the model <see cref=\"{sourceModel.Name}\"/>");
            classFormatter.AppendCodeLine(1, "/// </summary>");
            classFormatter.AppendCodeLine(1, $"[ModelConvert(typeof({sourceModel.Namespace}.{sourceModel.Name}))]");
            classFormatter.AppendCodeLine(1, "[ProtoContract()]");
            classFormatter.AppendCodeLine(1, $"public class {targetModelName}");
            classFormatter.AppendCodeLine(1, "{");


            var contentResults = await source.BuildGrpcModelContentAsync(rpcModelProject, appModelProject, sourceModel, targetModelName,
                targetNamespace, refreshedModels);
            if (contentResults?.Result == null)
            {
                throw new CodeFactoryException("Model content could not be generated, operation canceled.");
            }

            if (contentResults?.ProcessedModel != null) refreshedModels = processedModels;

            var contentFormatter = contentResults.Result;
            classFormatter.AppendCode(contentFormatter.ReturnSource());

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

            refreshedModels = refreshedModels.Add(new ModelProcessed(sourceModel, result));

            return new ModelProcessResult<CsClass>(refreshedModels, result);

        }

        /// <summary>
        /// Updates a Grpc model from a target class definition.
        /// </summary>
        /// <param name="source">CodeFactory action framework.</param>
        /// <param name="rpcModelProject">Grpc model project to update.</param>
        /// <param name="appModelProject">The project that hosts application model projects.</param>
        /// <param name="grpcModelSource">The source Grpc model to update.</param>
        /// <param name="sourceModel">The source model definition used to update the grpc model.</param>
        /// <param name="processedModels">List of models that have been processed.</param>
        /// <returns>The target implementation and the models that have been processed.</returns>
        /// <exception cref="CodeFactoryException">Exceptions that occur while processing the model.</exception>
        public static async Task<ModelProcessResult<CsClass>> UpdateGrpcModelAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject, 
            VsCSharpSource grpcModelSource, CsClass sourceModel, ImmutableList<ModelProcessed> processedModels)
        {

            if (rpcModelProject == null) throw new CodeFactoryException("No project for grpc was provided cannot update the model.");

            if (appModelProject == null) throw new CodeFactoryException("No project for the application models was provided cannot update the model.");

            if (grpcModelSource == null) throw new CodeFactoryException("No Grpc source was provided cannot complete operation.");

            if (sourceModel == null) throw new CodeFactoryException("No source model was provided cannot update Grpc model.");

            if (processedModels == null) throw new CodeFactoryException("No processed models was provided operation cannot complete.");

            CsClass grpcModel = grpcModelSource.SourceCode.Classes.FirstOrDefault();

            if (grpcModel == null) throw new CodeFactoryException("No grpc class was found from the supplied model source operation canceled.");

            var modelSource = grpcModelSource.SourceCode;

            if (!modelSource.HasUsingStatement("System")) modelSource = await modelSource.AddUsingStatementAsync("System");

            if (!modelSource.HasUsingStatement("System.Collections.Generic")) modelSource = await modelSource.AddUsingStatementAsync("System.Collections.Generic");

            if (!modelSource.HasUsingStatement("System.Linq")) modelSource = await modelSource.AddUsingStatementAsync("System.Linq");

            if (!modelSource.HasUsingStatement("System.Threading.Tasks")) modelSource = await modelSource.AddUsingStatementAsync("System.Threading.Tasks");

            if (!modelSource.HasUsingStatement("CommonDeliveryFramework")) modelSource = await modelSource.AddUsingStatementAsync("CommonDeliveryFramework");

            if (!modelSource.HasUsingStatement("CommonDeliveryFramework.Grpc")) modelSource = await modelSource.AddUsingStatementAsync("CommonDeliveryFramework.Grpc");

            if (!modelSource.HasUsingStatement(appModelProject.DefaultNamespace)) modelSource = await modelSource.AddUsingStatementAsync(appModelProject.DefaultNamespace);

            if (!modelSource.HasUsingStatement("ProtoBuf")) modelSource = await modelSource.AddUsingStatementAsync("ProtoBuf");

            grpcModel = modelSource.Classes.First();

            if (grpcModel == null) throw new CodeFactoryException("Could not load grpc data model operation canceled.");

            var properties =
                grpcModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public & !p.IsStatic);

            if (properties.Any())
            {

                foreach (var currentProperty in properties)
                {
                    var targetModel = grpcModel.GetModel(currentProperty.LookupPath);

                    if (targetModel == null) continue;

                    var targetProperty = targetModel as CsProperty;

                    if (targetProperty == null) continue;

                    modelSource = await targetProperty.DeleteAsync(grpcModel.SourceDocument);

                    grpcModel = modelSource.Classes.First();

                    if (grpcModel == null)
                        throw new CodeFactoryException("Could not load grpc data model operation canceled.");

                }

            }

            var methods = grpcModel.Methods.Where(m => m.Name == $"CreateAppModel" | m.Name == $"CreateGrpcModel");

            if (methods.Any())
            {
                foreach (var currentMethod in methods)
                {
                    var targetModel = grpcModel.GetModel(currentMethod.LookupPath);

                    if (targetModel == null) continue;

                    var targetMethod = targetModel as CsMethod;

                    if (targetMethod == null) continue;

                    modelSource = await targetMethod.DeleteAsync(grpcModel.SourceDocument);

                    grpcModel = modelSource.Classes.First();

                    if (grpcModel == null)
                        throw new CodeFactoryException("Could not load grpc data model operation canceled.");
                }
            }

            var result = await source.BuildGrpcModelContentAsync(rpcModelProject,appModelProject, sourceModel, grpcModel.Name,
                grpcModel.Namespace, processedModels);


            if (result?.ProcessedModel == null) throw new CodeFactoryException("Process models were not loaded cannot continue.");
            if (result?.Result == null) throw new CodeFactoryException("Cannot load model properties for grpc model cannot continue.");

            var content = result.Result;

            modelSource = await grpcModel.AddToEndAsync(content.ReturnSource());

            grpcModel = modelSource.Classes.First();

            if (grpcModel == null)
                throw new CodeFactoryException("Could not load grpc data model operation canceled.");

            var updatedModels = result.ProcessedModel.Add(new ModelProcessed(sourceModel, grpcModel));

            return new ModelProcessResult<CsClass>(updatedModels, grpcModel);
        }

        public static async Task<ModelProcessResult<SourceFormatter>> BuildGrpcModelContentAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject, CsClass sourceModel, 
            string targetModelName, string targetNamespace, ImmutableList<ModelProcessed> processedModels)
        {
            var targetProject = rpcModelProject;
            CsClass result = null;

            var refreshedModels = processedModels;
            if (refreshedModels == null) return null;

            if (rpcModelProject == null) return null;
            if (sourceModel == null) return null;

            if (string.IsNullOrEmpty(targetModelName)) return null;

            var properties = sourceModel.Properties.Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public);

            if (!properties.Any()) return null;

            if (targetNamespace == null) return null;

            SourceFormatter classFormatter = new SourceFormatter();


            bool sourceFirstParameter = true;
            SourceFormatter sourceCreateFormatter = new SourceFormatter();

            sourceCreateFormatter.AppendCodeLine(2, "/// <summary>");
            sourceCreateFormatter.AppendCodeLine(2, $"/// Creates an instance of the Grpc Model <see cref=\"{targetModelName}\"/> from a <see cref=\"{sourceModel.Name}\"/> model.");
            sourceCreateFormatter.AppendCodeLine(2, "/// </summary>");
            sourceCreateFormatter.AppendCodeLine(2, "/// <param name=\"source\">The model to transform into a Grpc Model.</param>");
            sourceCreateFormatter.AppendCodeLine(2, "/// <returns>Instance of the Grpc model.</returns>");
            sourceCreateFormatter.AppendCodeLine(2, $"public static {targetModelName} CreateGrpcModel({sourceModel.Name} source)");
            sourceCreateFormatter.AppendCodeLine(2, "{");

            bool destinationFirstParameter = true;
            SourceFormatter destinationCreateFormatter = new SourceFormatter();

            destinationCreateFormatter.AppendCodeLine(2, "/// <summary>");
            destinationCreateFormatter.AppendCodeLine(2, $"/// Creates an instance of the Model <see cref=\"{sourceModel.Name}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
            destinationCreateFormatter.AppendCodeLine(2, "/// </summary>");
            destinationCreateFormatter.AppendCodeLine(2, "// <returns>Instance of the model.</returns>");
            destinationCreateFormatter.AppendCodeLine(2, $"public {sourceModel.Name} CreateAppModel()");
            destinationCreateFormatter.AppendCodeLine(2, "{");

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

                if (property.HasAttributes)
                    //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                    foreach (var attributeSyntax in property.Attributes.CSharpFormatAttributeDeclarationEnumerator())
                    {
                        //Appending each attribute definition before the member definition.
                        classFormatter.AppendCodeLine(2, attributeSyntax);
                    }

                //Add the member declaration attribute for the serializer.
                classFormatter.AppendCodeLine(2, $"[ProtoMember({propertyCount})]");

                if (propertyType.IsGrpcSupportedType())
                {

                    //Using the formatter helper to generate a default property signature.
                    string propertySyntax = property.CSharpFormatDefaultPropertySignature();
                    classFormatter.AppendCodeLine(2, propertySyntax);
                    classFormatter.AppendCodeLine(2);

                    if (!sourceFirstParameter)
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name} ");
                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{property.Name} = source.{property.Name}");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name} ");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $"return new {sourceModel.Name}{{{property.Name} = {property.Name}");
                        destinationFirstParameter = false;
                    }
                    continue;
                }

                if (propertyType.IsGrpcSupportTypeList())
                {

                    //Using the formatter helper to generate a default property signature.
                    string propertySyntax = property.CSharpFormatDefaultPropertySignature();
                    classFormatter.AppendCodeLine(2, propertySyntax);
                    classFormatter.AppendCodeLine(2);

                    var listType = propertyType.GenericTypes.FirstOrDefault();

                    if (listType == null)
                    {
                        throw new CodeFactoryException(
                            $"Could not create generate the property '{propertyType.Name}', the generic parameters could not be accessed.");
                    }

                    if (!sourceFirstParameter)
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name} == null ? new List<{listType.Namespace}.{listType.Name}>() : source.{property.Name}");
                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{property.Name} = source.{property.Name} == null ? new List<{listType.Namespace}.{listType.Name}>() : source.{property.Name}");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name} == null ? new List<{listType.Namespace}.{listType.Name}>() : {property.Name}");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $"return new {sourceModel.Name}{{{property.Name} = {property.Name} == null ? new List<{listType.Namespace}.{listType.Name}>() : {property.Name}");
                        destinationFirstParameter = false;
                    }
                    continue;
                }

                if (propertyType.IsModel(appModelProject.DefaultNamespace))
                {

                    var targetAppModel = propertyType.GetClassModel();

                    var grpcClass = refreshedModels.FirstOrDefault(r =>
                            r.SourceClass.Namespace == targetAppModel.Namespace &
                            r.SourceClass.Name == targetAppModel.Name)
                        ?.TargetClass;

                    if (grpcClass == null)
                    {
                        var grpcClassResult = await source.RefreshGrpcModelAsync(targetProject, appModelProject, targetAppModel, refreshedModels);

                        if (grpcClassResult?.ProcessedModel != null) refreshedModels = grpcClassResult.ProcessedModel;
                        grpcClass = grpcClassResult?.Result;
                    }

                    if (grpcClass == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the property '{property.Name}' the supporting gRpc model for this property count not be created or found.");
                    }

                    refreshedModels = refreshedModels.Add(new ModelProcessed(targetAppModel, grpcClass));


                    classFormatter.AppendCodeLine(2, $"public {grpcClass.Namespace}.{grpcClass.Name} {property.Name} {{ get; set; }}");
                    classFormatter.AppendCodeLine(2);

                    if (!sourceFirstParameter)
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = {grpcClass.Namespace}.{grpcClass.Name}.{property.Name}.CreateGrpcModel(source.{property.Name})");
                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{property.Name} = {grpcClass.Namespace}.{grpcClass.Name}.{property.Name}.CreateGrpcModel(source.{property.Name})");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name}.GetAppModel()");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $"return new {sourceModel.Name}{{{property.Name} = {property.Name}.GetAppModel()");
                        destinationFirstParameter = false;
                    }
                    continue;

                }

                if (propertyType.IsGrpcModelList(appModelProject.DefaultNamespace))
                {

                    var appModelType =
                        propertyType.GenericTypes.FirstOrDefault(t => t.Namespace.StartsWith(appModelProject.DefaultNamespace));

                    if (appModelType == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the property '{property.Name}' the supporting gRpc model for this property count not be created or found.");
                    }

                    var targetAppModel = appModelType.GetClassModel();

                    if (targetAppModel == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the property '{property.Name}' the supporting gRpc model class data could not be loaded.");
                    }


                    var grpcClass = refreshedModels.FirstOrDefault(r =>
                            r.SourceClass.Namespace == targetAppModel.Namespace &
                            r.SourceClass.Name == targetAppModel.Name)
                        ?.TargetClass;

                    if (grpcClass == null)
                    {
                        var grpcClassResult = await source.RefreshGrpcModelAsync(targetProject, appModelProject, targetAppModel, refreshedModels);

                        if (grpcClassResult?.ProcessedModel != null) refreshedModels = grpcClassResult.ProcessedModel;
                        grpcClass = grpcClassResult?.Result;

                    }

                    if (grpcClass == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the property '{property.Name}' the supporting gRpc model for this property count not be created or found.");
                    }

                    refreshedModels = refreshedModels.Add(new ModelProcessed(targetAppModel, grpcClass));

                    classFormatter.AppendCodeLine(2, $"public List<{grpcClass.Namespace}.{grpcClass.Name}> {property.Name} {{ get; set; }}");
                    classFormatter.AppendCodeLine(2);

                    if (!sourceFirstParameter)
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name} == null ? new List<{grpcClass.Namespace}.{grpcClass.Name}>() : source.{property.Name}.Select(s => {grpcClass.Namespace}.{grpcClass.Name}.CreateGrpcModel(s)).ToList()");

                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3,
                            $"return new {targetModelName}{{ {property.Name} = source.{property.Name} == null ? new List<{grpcClass.Namespace}.{grpcClass.Name}>() : source.{property.Name}.Select(s => {grpcClass.Namespace}.{grpcClass.Name}.CreateGrpcModel(s)).ToList()");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name} == null ? new List<{targetAppModel.Namespace}.{targetAppModel.Name}>() :{property.Name}.Select(s => s.CreateAppModel()).ToList()");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3,
                            $"return new {sourceModel.Name}{{ {property.Name} = {property.Name} == null ? new List<{targetAppModel.Namespace}.{targetAppModel.Name}>() :{property.Name}.Select(s => s.CreateAppModel()).ToList()");
                        destinationFirstParameter = false;
                    }

                    continue;
                }

                throw new CodeFactoryException(
                    $"Cannot Create Model {targetModelName} the property '{property.Name}' has an unsupported type for model generation.");
            }


            sourceCreateFormatter.AppendCode(" };");
            sourceCreateFormatter.AppendCodeLine(2, "}");

            classFormatter.AppendCodeLine(2);
            classFormatter.AppendCode(sourceCreateFormatter.ReturnSource());

            destinationCreateFormatter.AppendCode(" };");
            destinationCreateFormatter.AppendCodeLine(2, "}");

            classFormatter.AppendCodeLine(2);
            classFormatter.AppendCode(destinationCreateFormatter.ReturnSource());

            return new ModelProcessResult<SourceFormatter>(refreshedModels, classFormatter);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks a type definition to see if the generic list implements a supported model type.
        /// </summary>
        /// <param name="source">The target type definition.</param>
        /// <param name="rootNamespace">The namespace the model must belong to.</param>
        /// <returns>True if the criteria for a supported model class are met, false otherwise.</returns>
        public static bool IsGrpcModelList(this CsType source, string rootNamespace)
        {
            if (source == null) return false;
            if (string.IsNullOrEmpty(rootNamespace)) return false;

            bool result = false;
            result = (source.Namespace == "System.Collections.Generic" & source.Name == "List" & source.IsGeneric);

            if (!result) return result;

            var type = source.GenericTypes.FirstOrDefault();

            result = type != null && type.Namespace.StartsWith(rootNamespace) && type.IsClass;

            return result;
        }

        /// <summary>
        /// Checks a type definition to see if the generic list implements supported types.
        /// </summary>
        /// <param name="source">The target type definition.</param>
        /// <returns>The list member is a supported type returns true, otherwise false.</returns>
        public static bool IsGrpcSupportTypeList(this CsType source)
        {
            if (source == null) return false;
            bool result = false;
            result = (source.Namespace == "System.Collections.Generic" & source.Name == "List" & source.IsGeneric);

            if (!result) return result;

            result = source.GenericTypes.All(g => g.IsGrpcSupportedType());

            return result;
        }

        /// <summary>
        /// Checks a type to determine if it supports serialization without a custom model definition.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>True if it supports serialization and false if not.</returns>
        public static bool IsGrpcSupportedType(this CsType source)
        {
            if (source == null) return false;

            bool result = false;

            result = source.IsWellKnownType;

            if (!result) result = source.Namespace == "System" & source.Name == "Guid";
            if (!result) result = source.Namespace == "System" & source.Name == "DateTime";

            return result;
        }

        #endregion

    }
}