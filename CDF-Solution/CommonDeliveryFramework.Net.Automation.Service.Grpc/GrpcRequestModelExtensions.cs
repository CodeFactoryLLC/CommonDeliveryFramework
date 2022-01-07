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
    public static partial class GrpcModelExtensions
    {
        
        #region Logic to get an existing service request model or create if it does not exist

        /// <summary>
        /// Get the target name of a service request class. 
        /// </summary>
        /// <param name="source">The source method used to generate the request.</param>
        /// <param name="serviceName">The target name of the service being implemented.</param>
        /// <returns>The name of the request model or null if no request model is needed for this method.</returns>
        public static string GetServiceRequestModelName(this CsMethod source, string serviceName)
        {
            if (source == null) throw new CodeFactoryException("No method was provided cannot get the name of the service request model.");
            if (serviceName == null) throw new CodeFactoryException("No service name was provided cannot get the name of the service request model.");

            if (!source.Parameters.Any()) return null;

            StringBuilder requestModelName = new StringBuilder(serviceName);
            foreach (var parameter in source.Parameters)
            {
                requestModelName.Append(parameter.Name.FormatProperCase());
            }
            requestModelName.Append("Request");
            return requestModelName.ToString();
        }


        /// <summary>
        /// Gets the Grpc request model for the provided source method, if it cannot be found it will create a new request model.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="rpcModelProject">The Grpc model project.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The source method to get the request model from.</param>
        /// <param name="contractName">The target contract name that is part of the request model name.</param>
        /// <returns>The existing or new model.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing, or a processing error.</exception>
        public static async Task<CsClass> GetGrpcRequestModelAsync(this IVsActions source, VsProject rpcModelProject,VsProject appModelProject, CsMethod sourceModel, string contractName)
        {
            if (source == null) throw new CodeFactoryException("No access to CodeFactory automation cannot get the Grpc request model.");
            if (rpcModelProject == null) throw new CodeFactoryException("No Grpc model project was provided, cannot get the Grpc request model.");
            if (string.IsNullOrEmpty(contractName)) throw new CodeFactoryException("No contract name was provided, cannot get the Grpc request model.");
            

            var requestModelName = sourceModel.GetServiceRequestModelName(contractName);
            if (string.IsNullOrEmpty(requestModelName)) throw new CodeFactoryException("The request name could not be created, cannot get the Grpc request model.");



            var requestModelNamespace = rpcModelProject.DefaultNamespace;

            if (string.IsNullOrEmpty(requestModelNamespace)) throw new CodeFactoryException("Could not get the target namespace for the request mode, cannot get the grpc request model");

            CsClass result = null;

            var children = await rpcModelProject.GetChildrenAsync(true, true);

            var targetSource = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(s => 
            {
                var classData = s.SourceCode?.Classes.FirstOrDefault();

                if (classData == null) return false;

                return classData.Name == requestModelName & classData.Namespace == requestModelNamespace;
            });

            if (targetSource != null) result = targetSource.SourceCode.Classes.FirstOrDefault();
            else result = await source.NewGrpcModelFromMethodDataAsync(rpcModelProject,appModelProject, sourceModel, requestModelName, requestModelNamespace, contractName, null);

            return result;
         }

        /// <summary>
        /// Implements a new data model with conversion from a source class definition.
        /// </summary>
        /// <param name="source">CodeFactory Actions that can be accessed.</param>
        /// <param name="rpcModelProject">The gRpc project to be added to.</param>
        /// <param name="appModelProject">The project that holds application models.</param>
        /// <param name="sourceModel">The source model to be implemented.</param>
        /// <param name="targetModelName">the name of the target model. </param>
        /// <param name="contractName">The contract name.</param>
        /// <param name="processedModels">List of models that all ready been processed, keeps from creating a runaway situation.</param>
        /// <param name="targetNamespace">The target namespace for the grpc model.</param>
        /// <returns>Returns once the model has been created.</returns>
        public static async Task<CsClass> NewGrpcModelFromMethodDataAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject,
             CsMethod sourceModel, string targetModelName, string targetNamespace,  string contractName, ImmutableList<ModelProcessed> processedModels)
        {

            if (source == null) throw new CodeFactoryException("No access to CodeFactory automation cannot create the Grpc request model.");
            if (rpcModelProject == null) throw new CodeFactoryException("No Grpc model project was provided, cannot create the Grpc request model.");
            if (appModelProject == null) throw new CodeFactoryException("No application model project was provided, cannot create the Grpc request model.");
            if (sourceModel == null) throw new CodeFactoryException("No method was provided, cannot create a Grpc request model.");
            if (string.IsNullOrEmpty(targetModelName)) throw new CodeFactoryException("The model name was not provided, cannot create the Grpc request model.");
            if (string.IsNullOrEmpty(targetNamespace)) throw new CodeFactoryException("The model namespace was not provided, cannot create the Grpc request model.");
            if (string.IsNullOrEmpty(contractName)) throw new CodeFactoryException("No contract name was provided, cannot create the Grpc request model.");
            

            var targetProject = rpcModelProject;
            CsClass result = null;

            var refreshedModels = processedModels ?? ImmutableList<ModelProcessed>.Empty;

            SourceFormatter classFormatter = new SourceFormatter();

           
            var  manualNamespaceManager = new ManualNamespaceManager(targetNamespace);

            manualNamespaceManager.AddUsingStatement("System");
            classFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            classFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            classFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            classFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Grpc");
            classFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            classFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement("ProtoBuf");
            classFormatter.AppendCodeLine(0, "using ProtoBuf;");

            
            classFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");

            classFormatter.AppendCodeLine(0, "{");

            classFormatter.AppendCodeLine(1, "/// <summary>");
            classFormatter.AppendCodeLine(1, $"/// Grpc model request class that is used with service requests that support the contract implementation of <see cref=\"I{contractName}\"/>");
            classFormatter.AppendCodeLine(1, "/// </summary>");
            classFormatter.AppendCodeLine(1, "[ProtoContract()]");
            classFormatter.AppendCodeLine(1, $"public class {targetModelName}");
            classFormatter.AppendCodeLine(1, "{");

            var manager = manualNamespaceManager.BuildNamespaceManager();

            var contentResults = await source.BuildMethodGrpcModelContentAsync(rpcModelProject,appModelProject, sourceModel, targetModelName,
                targetNamespace, refreshedModels,manager);

            if (contentResults?.Result == null)
            {
                throw new CodeFactoryException("Service request model content could not be generated, operation canceled.");
            }

            if (contentResults?.ProcessedModel != null) refreshedModels = processedModels;

            var contentFormatter = contentResults.Result;
            classFormatter.AppendCode(contentFormatter.ReturnSource());

            classFormatter.AppendCodeLine(1, "}");
            classFormatter.AppendCodeLine(0, "}");

            var document = await  rpcModelProject.AddDocumentAsync($"{targetModelName}.cs", classFormatter.ReturnSource());

            var modelSourceCode = await document.GetCSharpSourceModelAsync();

            result = modelSourceCode.Classes.FirstOrDefault();

            return result;

        }

        /// <summary>
        /// Builds a Grpc model content from a source method definition.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="rpcModelProject">The Grpc models project to update.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The source class model to create into a Grpc model.</param>
        /// <param name="targetModelName">The name of the target Grpc model.</param>
        /// <param name="targetNamespace">The namespace for the Grpc model.</param>
        /// <param name="processedModels">Additional dependent grpc models that have been processed that support this model.</param>
        /// <param name="manager">The namespace manager that shortens type definitions.</param>
        /// <returns>The content for a Grpc model.</returns>
        /// <exception cref="CodeFactoryException">Error that occur while processing the model.</exception>
        public static async Task<ModelProcessResult<SourceFormatter>> BuildMethodGrpcModelContentAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject, CsMethod sourceModel,
            string targetModelName, string targetNamespace, ImmutableList<ModelProcessed> processedModels, NamespaceManager manager)
        {

            if (source == null) throw new CodeFactoryException("CodeFactory automation was not found, cannot build the request model content");
            if (rpcModelProject == null) throw new CodeFactoryException("The Grpc model project was not provided, cannot build the request model content");
            if (appModelProject == null) throw new CodeFactoryException("The application model project was not provided, cannot build the request model content");
            if (sourceModel == null) throw new CodeFactoryException("No source method was provided, cannot build the request model content");
            if (string.IsNullOrEmpty(targetModelName)) throw new CodeFactoryException("No target model name was provided, cannot build the request model content");
            if (string.IsNullOrEmpty(targetNamespace)) throw new CodeFactoryException("No target namespace was provided, cannot build the request model content");

            var parameters = sourceModel.Parameters;

            if (!parameters.Any()) throw new CodeFactoryException("The source method has not parameters, cannot build the request model content");

            var targetProject = rpcModelProject;
            var refreshedModels = processedModels ?? ImmutableList<ModelProcessed>.Empty;

            SourceFormatter classFormatter = new SourceFormatter();


            bool sourceFirstParameter = true;
            SourceFormatter createSourceFormatter = new SourceFormatter();
            StringBuilder createSourceBuilder = new StringBuilder($"public static {targetModelName} CreateGrpcModel(");

            SourceFormatter getPropertyFormatter = new SourceFormatter();

            int parameterCount = 0;
            foreach (var parameter in parameters)
            {
                parameterCount++;

                var parameterType = parameter.ParameterType;


                //Add the member declaration attribute for the serializer.
                classFormatter.AppendCodeLine(2, $"[ProtoMember({parameterCount})]");

                if (parameterType.IsGrpcSupportedType())
                {

                    classFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} {parameter.Name.FormatProperCase()} {{ get; set;}}");
                    classFormatter.AppendCodeLine(2);

                    if (!sourceFirstParameter)
                    {
                        createSourceBuilder.Append($", {parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $",{parameter.Name.FormatProperCase()} = {parameter.Name} ");
                        
                    }
                    else
                    {
                        createSourceBuilder.Append($"{parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{parameter.Name.FormatProperCase()} = {parameter.Name} ");
                        sourceFirstParameter = false;

                    }

                    getPropertyFormatter.AppendCodeLine(2, "/// <summary>");
                    getPropertyFormatter.AppendCodeLine(2, $"/// Gets the value from property model <see cref=\"{parameter.Name.FormatProperCase()}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
                    getPropertyFormatter.AppendCodeLine(2, "/// </summary>");
                    getPropertyFormatter.AppendCodeLine(2, "// <returns>the app supported value</returns>");
                    getPropertyFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} GetAppValueFor{parameter.Name.FormatProperCase()}()");
                    getPropertyFormatter.AppendCodeLine(2, "{");
                    getPropertyFormatter.AppendCodeLine(3, $"return {parameter.Name.FormatProperCase()};");
                    getPropertyFormatter.AppendCodeLine(2, "}");
                    getPropertyFormatter.AppendCodeLine(2);

                    continue;
                }

                if (parameterType.IsGrpcSupportTypeList())
                {

                    classFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} {parameter.Name.FormatProperCase()} {{ get; set;}}");
                    classFormatter.AppendCodeLine(2);

                    var listType = parameterType.GenericTypes.FirstOrDefault();

                    if (listType == null)
                    {
                        throw new CodeFactoryException(
                            $"Could not create generate the property '{parameterType.Name}', the generic parameters could not be accessed.");
                    }

                    if (!sourceFirstParameter)
                    {
                        createSourceBuilder.Append($", {parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $",{parameter.Name.FormatProperCase()} = {parameter.Name} ?? new List<{listType.CSharpFormatTypeName(manager)}>()");

                    }
                    else
                    {
                        createSourceBuilder.Append($"{parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{parameter.Name.FormatProperCase()} = {parameter.Name}?? new List<{listType.CSharpFormatTypeName(manager)}>()");
                        sourceFirstParameter = false;

                    }

                    getPropertyFormatter.AppendCodeLine(2, "/// <summary>");
                    getPropertyFormatter.AppendCodeLine(2, $"/// Gets the value from property model <see cref=\"{parameter.Name.FormatProperCase()}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
                    getPropertyFormatter.AppendCodeLine(2, "/// </summary>");
                    getPropertyFormatter.AppendCodeLine(2, "// <returns>the app supported value</returns>");
                    getPropertyFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} GetAppValueFor{parameter.Name.FormatProperCase()}()");
                    getPropertyFormatter.AppendCodeLine(2, "{");
                    getPropertyFormatter.AppendCodeLine(3, $"return {parameter.Name.FormatProperCase()}?? new List<{listType.CSharpFormatTypeName(manager)}>();");
                    getPropertyFormatter.AppendCodeLine(2, "}");
                    getPropertyFormatter.AppendCodeLine(2);

                    continue;
                }

                if (parameterType.IsModel(appModelProject.DefaultNamespace))
                {

                    var targetAppModel = parameterType.GetClassModel();

                    var grpcClass = refreshedModels.FirstOrDefault(r =>
                            r.SourceClass.Namespace == targetAppModel.Namespace &
                            r.SourceClass.Name == targetAppModel.Name)
                        ?.TargetClass;

                    if (grpcClass == null)
                    {
                        var grpcClassResult = await source.GetGrpcModelAsync(targetProject,appModelProject, targetAppModel, refreshedModels);

                        if (grpcClassResult?.ProcessedModel != null) refreshedModels = grpcClassResult.ProcessedModel;
                        grpcClass = grpcClassResult?.Result;
                    }

                    if (grpcClass == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the parameter '{parameter.Name}' the supporting gRpc model for this parameter can not be created or found.");
                    }

                    refreshedModels = refreshedModels.Add(new ModelProcessed(targetAppModel, grpcClass));


                    classFormatter.AppendCodeLine(2, $"public {grpcClass.CSharpFormatBaseTypeName(manager)} {parameter.Name.FormatProperCase()} {{ get; set; }}");
                    classFormatter.AppendCodeLine(2);


                    if (!sourceFirstParameter)
                    {
                        createSourceBuilder.Append($", {parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $",{parameter.Name.FormatProperCase()} = {grpcClass.CSharpFormatBaseTypeName(manager)}.CreateGrpcModel({parameter.Name}) ");

                    }
                    else
                    {
                        createSourceBuilder.Append($"{parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{parameter.Name.FormatProperCase()} = {grpcClass.CSharpFormatBaseTypeName(manager)}.CreateGrpcModel({parameter.Name}) ");
                        sourceFirstParameter = false;

                    }

                    getPropertyFormatter.AppendCodeLine(2, "/// <summary>");
                    getPropertyFormatter.AppendCodeLine(2, $"/// Gets the value from property model <see cref=\"{parameter.Name.FormatProperCase()}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
                    getPropertyFormatter.AppendCodeLine(2, "/// </summary>");
                    getPropertyFormatter.AppendCodeLine(2, "// <returns>the app supported value</returns>");
                    getPropertyFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} GetAppValueFor{parameter.Name.FormatProperCase()}()");
                    getPropertyFormatter.AppendCodeLine(2, "{");
                    getPropertyFormatter.AppendCodeLine(3, $"return {parameter.Name.FormatProperCase()}.CreateAppModel();");
                    getPropertyFormatter.AppendCodeLine(2, "}");
                    getPropertyFormatter.AppendCodeLine(2);

               
                    continue;

                }

                if (parameterType.IsGrpcModelList(appModelProject.DefaultNamespace))
                {

                    var appModelType =
                        parameterType.GenericTypes.FirstOrDefault(t => t.Namespace.StartsWith(appModelProject.DefaultNamespace));

                    if (appModelType == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the parameter '{parameter.Name}' the supporting gRpc model for this parameter could not be created or found.");
                    }

                    var targetAppModel = appModelType.GetClassModel();

                    if (targetAppModel == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the parameter '{parameter.Name}' the supporting gRpc model class data could not be loaded.");
                    }


                    var grpcClass = refreshedModels.FirstOrDefault(r =>
                            r.SourceClass.Namespace == targetAppModel.Namespace &
                            r.SourceClass.Name == targetAppModel.Name)
                        ?.TargetClass;

                    if (grpcClass == null)
                    {
                        var grpcClassResult = await source.GetGrpcModelAsync(targetProject,appModelProject, targetAppModel, refreshedModels);

                        if (grpcClassResult?.ProcessedModel != null) refreshedModels = grpcClassResult.ProcessedModel;
                        grpcClass = grpcClassResult?.Result;

                    }

                    if (grpcClass == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the parameter '{parameter.Name}' the supporting gRpc model for this parameter could not be created or found.");
                    }

                    refreshedModels = refreshedModels.Add(new ModelProcessed(targetAppModel, grpcClass));

                    classFormatter.AppendCodeLine(2, $"public List<{grpcClass.CSharpFormatBaseTypeName(manager)}> {parameter.Name.FormatProperCase()} {{ get; set; }}");
                    classFormatter.AppendCodeLine(2);


                    if (!sourceFirstParameter)
                    {
                        createSourceBuilder.Append($", {parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $",{parameter.Name.FormatProperCase()} = {parameter.Name} == null ? new List<{grpcClass.CSharpFormatBaseTypeName(manager)}>() : source.{parameter.Name}.Select(s => {grpcClass.CSharpFormatBaseTypeName(manager)}.CreateGrpcModel(s)).ToList())");

                    }
                    else
                    {
                        createSourceBuilder.Append($"{parameterType.CSharpFormatTypeName(manager)} {parameter.Name}");
                        createSourceFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{parameter.Name.FormatProperCase()} = {parameter.Name} == null ? new List<{grpcClass.CSharpFormatBaseTypeName(manager)}>() : source.{parameter.Name}.Select(s => {grpcClass.CSharpFormatBaseTypeName(manager)}.CreateGrpcModel(s)).ToList())");
                        sourceFirstParameter = false;

                    }

                    getPropertyFormatter.AppendCodeLine(2, "/// <summary>");
                    getPropertyFormatter.AppendCodeLine(2, $"/// Gets the value from property model <see cref=\"{parameter.Name.FormatProperCase()}\"/> from the current instance of the Grpc model <see cref=\"{targetModelName}\"/>.");
                    getPropertyFormatter.AppendCodeLine(2, "/// </summary>");
                    getPropertyFormatter.AppendCodeLine(2, "// <returns>the app supported value</returns>");
                    getPropertyFormatter.AppendCodeLine(2, $"public {parameterType.CSharpFormatTypeName(manager)} GetAppValueFor{parameter.Name.FormatProperCase()}()");
                    getPropertyFormatter.AppendCodeLine(2, "{");
                    getPropertyFormatter.AppendCodeLine(3, $"return {parameter.Name.FormatProperCase()} == null ? new {parameterType.CSharpFormatTypeName(manager)}() : {parameter.Name.FormatProperCase()}.Select(s => s.CreateAppModel()).ToList()); ");
                    getPropertyFormatter.AppendCodeLine(2, "}");
                    getPropertyFormatter.AppendCodeLine(2);


                    continue;
                }

                throw new CodeFactoryException(
                    $"Cannot Create Model {targetModelName} the parameter '{parameter.Name}' has an unsupported type for model generation.");

            }

            createSourceFormatter.AppendCode("};");
            createSourceBuilder.Append(")");
            createSourceFormatter.AppendCodeLine(2, "}");
            createSourceFormatter.AppendCodeLine(2);

            classFormatter.AppendCodeLine(2, "/// <summary>");
            classFormatter.AppendCodeLine(2, $"/// Creates an instance of the request model <see cref=\"{targetModelName}\"/>");
            classFormatter.AppendCodeLine(2, "/// </summary>");
            classFormatter.AppendCodeLine(2, "/// <returns>Instance of the Grpc model.</returns>");
            classFormatter.AppendCodeLine(2, createSourceBuilder.ToString());
            classFormatter.AppendCodeLine(2, "{");
            classFormatter.AppendCodeLine(0, createSourceFormatter.ReturnSource());
            classFormatter.AppendCodeLine(0, getPropertyFormatter.ReturnSource());

            return new ModelProcessResult<SourceFormatter>(refreshedModels, classFormatter);
        }


        #endregion

        #region Logic to get grpc models that implement app models, or create them if they are missing

        /// <summary>
        /// Find a gRpc model from a target project or creates a new Grpc model if it does not exist.
        /// </summary>
        /// <param name="source">The CodeFactory actions API starting from.</param>
        /// <param name="rpcModelProject">The target gRpc project to search.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The target APP model to look for being transformed in gRpc.</param>
        /// <param name="processedModels">List of models that all ready been processed, keeps from creating a runaway situation.</param>
        /// <returns>Returns the implemented gRpc class that support the source model, or null.</returns>
        public static async Task<ModelProcessResult<CsClass>> GetGrpcModelAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject, CsClass sourceModel, ImmutableList<ModelProcessed> processedModels = null)
        {
            if (source == null) return null;
            if (sourceModel == null) return null;

            var refreshedModels = processedModels ?? ImmutableList<ModelProcessed>.Empty;

            CsClass result = null;

            string targetModelName = $"{sourceModel.Name}{ServiceRpcSuffix}";

            var sourceCodeFile = await rpcModelProject.FindModel(sourceModel, targetModelName);

            if (sourceCodeFile == null)
            {
                var processResult = await source.NewGrpcModelFromClassDataAsync(rpcModelProject, appModelProject,sourceModel, targetModelName, refreshedModels);
                if (processResult?.ProcessedModel != null) refreshedModels = processedModels;
                result = processResult?.Result;
            }
            else
            {
                result = sourceCodeFile.SourceCode.Classes.FirstOrDefault();
                refreshedModels.Add(new ModelProcessed(sourceModel, result));
            }

            return new ModelProcessResult<CsClass>(refreshedModels, result);
        }

        /// <summary>
        /// Implements a new data model with conversion from a source class definition.
        /// </summary>
        /// <param name="source">CodeFactory Actions that can be accessed.</param>
        /// <param name="rpcModelProject">The gRpc project to be added to.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The source model to be implemented.</param>
        /// <param name="targetModelName">the name of the target model. </param>
        /// <param name="processedModels">List of models that all ready been processed, keeps from creating a runaway situation.</param>
        /// <returns>Returns once the model has been created.</returns>
        public static async Task<ModelProcessResult<CsClass>> NewGrpcModelFromClassDataAsync(this IVsActions source, VsProject rpcModelProject, VsProject appModelProject
            , CsClass sourceModel, string targetModelName, ImmutableList<ModelProcessed> processedModels)
        {

            var targetProject = rpcModelProject;
            CsClass result = null;

            var refreshedModels = processedModels;
            if (refreshedModels == null) return null;
            if (rpcModelProject == null) return null;
            if (appModelProject == null) return null;
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



            var manualNamespaceManager = new ManualNamespaceManager(targetNamespace);
            manualNamespaceManager.AddUsingStatement("System");
            classFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            classFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            classFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            classFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            classFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            classFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement("ProtoBuf");
            classFormatter.AppendCodeLine(0, "using ProtoBuf;");
            if (sourceModel.Namespace != appModelProject.DefaultNamespace)
            {
                manualNamespaceManager.AddUsingStatement(sourceModel.Namespace);
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

            var manager = manualNamespaceManager.BuildNamespaceManager();

            var contentResults = await source.BuildClassGrpcModelContentAsync(rpcModelProject,appModelProject, sourceModel, targetModelName,
                targetNamespace, refreshedModels,manager);
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
        /// Builds a Grpc model content from a source class definition.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="rpcModelProject">The Grpc models project to update.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="sourceModel">The source class model to create into a Grpc model.</param>
        /// <param name="targetModelName">The name of the target Grpc model.</param>
        /// <param name="targetNamespace">The namespace for the Grpc model.</param>
        /// <param name="processedModels">Additional dependent grpc models that have been processed that support this model.</param>
        /// <param name="manager">Namespace manager used to shorten type definitions.</param>
        /// <returns>The content for a Grpc model.</returns>
        /// <exception cref="CodeFactoryException">Error that occur while processing the model.</exception>
        public static async Task<ModelProcessResult<SourceFormatter>> BuildClassGrpcModelContentAsync(this IVsActions source, VsProject rpcModelProject,VsProject appModelProject, CsClass sourceModel,
            string targetModelName, string targetNamespace, ImmutableList<ModelProcessed> processedModels, NamespaceManager manager)
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
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name} == null ? new List<{listType.CSharpFormatTypeName(manager)}>() : source.{property.Name}");
                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{property.Name} = source.{property.Name} == null ? new List<{listType.CSharpFormatTypeName(manager)}>() : source.{property.Name}");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name} == null ? new List<{listType.CSharpFormatTypeName(manager)}>() : {property.Name}");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $"return new {sourceModel.Name}{{{property.Name} = {property.Name} == null ? new List<{listType.CSharpFormatTypeName(manager)}>() : {property.Name}");
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
                        var grpcClassResult = await source.GetGrpcModelAsync(targetProject, appModelProject, targetAppModel, refreshedModels);

                        if (grpcClassResult?.ProcessedModel != null) refreshedModels = grpcClassResult.ProcessedModel;
                        grpcClass = grpcClassResult?.Result;
                    }

                    if (grpcClass == null)
                    {
                        throw new CodeFactoryException(
                            $"Cannot Create Model {targetModelName} the property '{property.Name}' the supporting gRpc model for this property count not be created or found.");
                    }

                    refreshedModels = refreshedModels.Add(new ModelProcessed(targetAppModel, grpcClass));


                    classFormatter.AppendCodeLine(2, $"public {grpcClass.CSharpFormatBaseTypeName(manager)} {property.Name} {{ get; set; }}");
                    classFormatter.AppendCodeLine(2);

                    if (!sourceFirstParameter)
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = {grpcClass.CSharpFormatBaseTypeName(manager)}.{property.Name}.CreateGrpcModel(source.{property.Name})");
                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3, $"return new {targetModelName}{{{property.Name} = {grpcClass.CSharpFormatBaseTypeName(manager)}.{property.Name}.CreateGrpcModel(source.{property.Name})");
                        sourceFirstParameter = false;
                    }

                    if (!destinationFirstParameter)
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $", {property.Name} = {property.Name}.CreateAppModel()");
                    }
                    else
                    {
                        destinationCreateFormatter.AppendCodeLine(3, $"return new {sourceModel.Name}{{{property.Name} = {property.Name}.CreateAppModel()");
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
                        var grpcClassResult = await source.GetGrpcModelAsync(targetProject,appModelProject, targetAppModel, refreshedModels);

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
                        sourceCreateFormatter.AppendCodeLine(3, $", {property.Name} = source.{property.Name} == null ? new List<{grpcClass.Namespace}.{grpcClass.Name}>() : source.{property.Name}.Select(s => {grpcClass.Namespace}.{grpcClass.Name}..CreateGrpcModel(s)).ToList()");

                    }
                    else
                    {
                        sourceCreateFormatter.AppendCodeLine(3,
                            $"return new {targetModelName}{{ {property.Name} = source.{property.Name} == null ? new List<{grpcClass.Namespace}.{grpcClass.Name}>() : source.{property.Name}.Select(s => {grpcClass.Namespace}.{grpcClass.Name}..CreateGrpcModel(s)).ToList()");
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
    }
}