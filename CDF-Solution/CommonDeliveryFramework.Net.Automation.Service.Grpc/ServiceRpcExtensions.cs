using System;
using System.Collections.Generic;
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
    /// Extensions and supporting logic for managing service rpc models and service code.
    /// </summary>
    public static class ServiceRpcExtensions
    {
        /// <summary>
        /// Updates a Grpc service implementation to be in sync with the supporting service contract.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="serviceContract">The Grpc service contract interface definition.</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <param name="rpcProject">The project that hosts the rpc service logic.</param>
        /// <param name="logicProject">The project that hosts the application logic.</param>
        /// <returns>Updated class definition of the implemented service.</returns>
        public static async Task<CsClass> UpdateGrpcServiceAsync(this IVsActions source, CsInterface serviceContract, CsInterface logicContract,VsProject appModelProject, VsProject rpcModelProject, VsProject rpcProject, VsProject logicProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the service.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided cannot update the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the service.");


            VsProject serviceProject = rpcProject;

            if (serviceProject == null) throw new CodeFactoryException("Could not load the Grpc service project, cannot update the service.");

            var children = await serviceProject.GetChildrenAsync(false, false);

            var serviceFolder = children.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>().FirstOrDefault(f => f.Name == "Services");

            if (serviceFolder == null) serviceFolder = await serviceProject.AddProjectFolderAsync("Services");

            if (serviceFolder == null) throw new CodeFactoryException("Could not access the 'Services' folder from the Grpc project cannot update the service.");

            var serviceName = serviceContract.GetClassName();

            var folderChildren = await serviceFolder.GetChildrenAsync(true, true);

            var serviceSource = await serviceProject.FindCSharpSourceByClassNameAsync(serviceName);

            return serviceSource == null ? await source.NewGrpcServiceAsync(serviceContract,logicContract,appModelProject,rpcModelProject,rpcProject,logicProject) : await source.UpdateGrpcServiceImplementationAsync(serviceSource,serviceContract,logicContract,rpcModelProject);
        }

        /// <summary>
        /// Create a new Grpc service implementation to be in sync with the supporting service contract.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="serviceContract">The Grpc service contract interface definition.</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <param name="rpcProject">The project that hosts the rpc service logic.</param>
        /// <param name="logicProject">The project that hosts the application logic.</param>
        /// <returns>Created class definition of the implemented service.</returns>
        public static async Task<CsClass> NewGrpcServiceAsync(this IVsActions source, CsInterface serviceContract, CsInterface logicContract, VsProject appModelProject, VsProject rpcModelProject, VsProject rpcProject, VsProject logicProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided cannot create the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service.");


            VsProject serviceProject = rpcProject;

            if (serviceProject == null) throw new CodeFactoryException("Could not load the Grpc service project cannot update the service.");

            var children = await serviceProject.GetChildrenAsync(false, false);

            var serviceFolder = children.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>().FirstOrDefault(f => f.Name == "Services");

            if (serviceFolder == null) serviceFolder = await serviceProject.AddProjectFolderAsync("Services");

            if (serviceFolder == null) throw new CodeFactoryException("Could not access the 'Services' folder from the Grpc project cannot update the service.");

            var sourceNamespace = await serviceFolder.GetCSharpNamespaceAsync();
            if (string.IsNullOrEmpty(sourceNamespace)) throw new CodeFactoryException("Could not load the target namespace for the Grpc service.");

            var serviceName = serviceContract.GetClassName();

            var manualNamespaceManager = new ManualNamespaceManager(sourceNamespace);
            var sourceFormatter = new SourceFormatter();

            manualNamespaceManager.AddUsingStatement("System");
            sourceFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Text");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("Microsoft.Extensions.Logging");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            manualNamespaceManager.AddUsingStatement("ProtoBuf.Grpc");
            sourceFormatter.AppendCodeLine(0, "using ProtoBuf.Grpc;");
            manualNamespaceManager.AddUsingStatement("ProtoBuf.Grpc.Configuration");
            sourceFormatter.AppendCodeLine(0, "using ProtoBuf.Grpc.Configuration;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Grpc");
            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manualNamespaceManager.AddUsingStatement(serviceContract.Namespace);
            sourceFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            sourceFormatter.AppendCodeLine(0, $"using {rpcModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(rpcModelProject.DefaultNamespace);
            sourceFormatter.AppendCodeLine(0, $"using {logicProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(logicProject.DefaultNamespace);


            var manager = manualNamespaceManager.BuildNamespaceManager();


            sourceFormatter.AppendCodeLine(0, $"namespace {sourceNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, $"public partial class {serviceName}:{serviceContract.Namespace}.{serviceContract.Name}");
            sourceFormatter.AppendCodeLine(1, "{");

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logger used for the class.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            sourceFormatter.AppendCodeLine(2);

            var logicVariable = $"_{logicContract.GetClassName().FormatCamelCase()}";

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logic class supporting the service implementation.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"private readonly {logicContract.Namespace}.{logicContract.Name} {logicVariable};");
            sourceFormatter.AppendCodeLine(2);


            var logicParameter = $"{logicContract.GetClassName().FormatCamelCase()}";

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, $"/// Creates a instance of the service class <see cref=\"{serviceName}\"/>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger that supports this service class.</param>");
            sourceFormatter.AppendCodeLine(2, $"/// <param name=\"{logicParameter}\">Logic contract implemented by this service class.</param>");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"public {serviceName}(ILogger<{serviceName}> logger,{logicContract.Namespace}.{logicContract.Name} {logicParameter})");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger = logger;");
            sourceFormatter.AppendCodeLine(3, $"{logicVariable} = {logicParameter};");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);

            List<Tuple<CsMethod,CsMethod>> serviceMethods = new List<Tuple<CsMethod,CsMethod>>();

            foreach (CsMethod method in logicContract.Methods)
            {
                string methodName = method.GetServiceMethodRpcName(serviceName);
                var serviceMethod = serviceContract.Methods.FirstOrDefault(m => m.Name == methodName);

                if (serviceMethod != null) serviceMethods.Add(new Tuple<CsMethod, CsMethod>(serviceMethod, method));
            }

            if (serviceMethods.Any()) sourceFormatter.AppendCode(await source.BuildServiceLogicAsync(serviceMethods, serviceContract, logicContract,rpcModelProject,manager));

            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            var sourceDocument = await serviceFolder.AddDocumentAsync($"{serviceName}.cs",sourceFormatter.ReturnSource());

            if (sourceDocument == null) throw new CodeFactoryException($"Was not able to load the created service implementation for '{serviceName}'");

            var serviceSource = await sourceDocument.GetCSharpSourceModelAsync();

            if (serviceSource == null) throw new CodeFactoryException($"Was not able to load the created service source code implementation for '{serviceName}'");


            return serviceSource.Classes.FirstOrDefault();
        }

        /// <summary>
        /// Updates a Grpc service implementation to be in sync with the supporting service contract.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="serviceSource">The source code for service implementation.</param>
        /// <param name="serviceContract">The Grpc service contract interface definition.</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <returns>Updated class definition of the implemented service.</returns>
        public static async Task<CsClass> UpdateGrpcServiceImplementationAsync(this IVsActions source,VsCSharpSource serviceSource,  CsInterface serviceContract, CsInterface logicContract,VsProject rpcModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the service.");
            if (serviceSource == null) throw new CodeFactoryException("No service source was provided cannot update the service.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided cannot update the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the service.");


            var result = serviceSource.SourceCode.Classes.FirstOrDefault();

            if (result == null) throw new CodeFactoryException("Cannot load the service class definition cannot update the service.");

            List<Tuple<CsMethod, CsMethod>> serviceMethods = new List<Tuple<CsMethod, CsMethod>>();

            foreach (CsMethod method in logicContract.Methods)
            {
                string methodName = method.GetServiceMethodRpcName(result.Name);
                var serviceMethod = serviceContract.Methods.FirstOrDefault(m => m.Name == methodName);

                if (serviceMethod != null & result.Methods.FirstOrDefault(m => m.Name == methodName) == null) serviceMethods.Add(new Tuple<CsMethod, CsMethod>(serviceMethod, method));
            }

            if (serviceMethods.Any())
            {
                var manager = new NamespaceManager(serviceSource.SourceCode.NamespaceReferences,serviceSource.SourceCode.Namespaces.FirstOrDefault()?.Name);
                var serviceContent = await source.BuildServiceLogicAsync(serviceMethods, serviceContract, logicContract,rpcModelProject,manager);

                var updatedSource = await result.AddToEndAsync(serviceContent);

                result = updatedSource.Classes.FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// Builds Grpc service logic implementation for a target service and logic contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="serviceMethods">The service methods and the target logic method that supports the service implementation.</param>
        /// <param name="serviceContract">The service contract being implemented.</param>
        /// <param name="logicContract">The logic contract that is being supported by the service contract.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <param name="manager">Namespace manager used for formatting type definitions.</param>
        /// <returns>Formatted service logic.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is not provided, or a logic error occurs.</exception>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task<string> BuildServiceLogicAsync(this IVsActions source,List<Tuple<CsMethod,CsMethod>> serviceMethods, CsInterface serviceContract, CsInterface logicContract,VsProject rpcModelProject,NamespaceManager manager)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service logic.");
            if (serviceMethods == null) throw new CodeFactoryException("No service methods were provided cannot create service logic.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided cannot create the service logic.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service logic.");

            var logicVariable = $"_{logicContract.GetClassName().FormatCamelCase()}";

            SourceFormatter logicFormatter = new SourceFormatter();

            foreach (var methods in serviceMethods)
            {
                var serviceMethod = methods.Item1;
                if (serviceMethod == null) throw new CodeFactoryException("Could not load the service method data, cannot create service logic.");
                var logicMethod = methods.Item2;
                if (logicMethod == null) throw new CodeFactoryException("Could not load the logic method data, cannot create service logic.");

                var serviceResult = serviceMethod.ReturnType?.GenericTypes.FirstOrDefault(t => t.Namespace == "CommonDeliveryFramework.Grpc" & t.Name == "ServiceResult");

                bool isServiceResult = serviceResult != null;

                if (!isServiceResult) if (serviceMethod.ReturnType?.GenericTypes.FirstOrDefault(t => t.Namespace == "CommonDeliveryFramework.Grpc" & t.Name == "NoDataResult") == null) throw new CodeFactoryException($"The service method '{serviceMethod.Name}' return type of '{serviceMethod.ReturnType.CSharpFormatTypeName()}' is not supported for generation.");

                string returnTypeName = null;
                CsType returnType = null;

                if (isServiceResult)
                {
                    returnType = serviceResult.GenericTypes.FirstOrDefault();

                    if (returnType == null) throw new CodeFactoryException($"Could not load the return type for the service method '{serviceMethod.Name}'");

                    returnTypeName = returnType.CSharpFormatTypeName(manager);
                }

                bool hasRequest = serviceMethod.Parameters.Count == 2;

                CsParameter requestParameter = null;

                CsClass requestClass = null;

                if (hasRequest) 
                {
                    requestParameter = serviceMethod.Parameters[0];
                    if (requestParameter == null) throw new CodeFactoryException($"Could not load the request parameter for the service method'{serviceMethod.Name}'.");

                    requestClass = requestParameter.ParameterType.GetClassModel();
                    if (requestClass == null) throw new CodeFactoryException($"Could not load the request class information for the service method '{serviceMethod.Name}'.");
                }



                if (serviceMethod.HasDocumentation)
                {
                    var doc = serviceMethod.CSharpFormatXmlDocumentationEnumerator();

                    foreach (var docLine in doc)
                    {
                       logicFormatter.AppendCodeLine(2,docLine);
                    }
                }

                if (serviceMethod.HasAttributes)
                {
                    foreach (var attributeData in serviceMethod.Attributes)
                    {
                        logicFormatter.AppendCodeLine(2, attributeData.CSharpFormatAttributeSignature(manager));
                    }
                }

                logicFormatter.AppendCodeLine(2, serviceMethod.CSharpFormatMethodSignature(manager));
                logicFormatter.AppendCodeLine(2, "{");
                logicFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
                logicFormatter.AppendCodeLine(3);

                if (hasRequest)
                {
                    logicFormatter.AppendCodeLine(3, $"if({requestParameter.Name} == null)");
                    logicFormatter.AppendCodeLine(3, "{");
                    logicFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({requestParameter.Name})}} was not provided. Will raise an argument exception\");");
                    logicFormatter.AppendCodeLine(4, $" var validationError = new ValidationException(nameof({requestParameter.Name}));");
                    logicFormatter.AppendCodeLine(4, $"_logger.InformationExitLog();");
                    logicFormatter.AppendCodeLine(4, isServiceResult ? $"return ServiceResult<{returnTypeName}>.CreateError(validationError);" : $"return NoDataResult.CreateError(validationError);");
                    logicFormatter.AppendCodeLine(3, "}");
                    logicFormatter.AppendCodeLine(3);
                }

                if (isServiceResult)
                {

                    var logicReturnType = logicMethod.ReturnType;

                    if (logicReturnType.Namespace == "System.Threading.Tasks" & logicReturnType.Name == "Task" & logicReturnType.HasStrongTypesInGenerics) logicReturnType = logicMethod.ReturnType.GenericTypes.FirstOrDefault();

                    if (logicReturnType == null) throw new CodeFactoryException($"Could not load the return type from the logic contract for service method '{serviceMethod.Name}'.");
                    logicFormatter.AppendCodeLine(3, logicMethod.ReturnType.IsClass ? $"{logicReturnType.CSharpFormatTypeName(manager)} result = null;": $"{logicReturnType.CSharpFormatTypeName(manager)} result;");
                    logicFormatter.AppendCodeLine(3);
                }

                logicFormatter.AppendCodeLine(3,"try");
                logicFormatter.AppendCodeLine(3, "{");

                if (!hasRequest)
                {
                    logicFormatter.AppendCodeLine(4, isServiceResult ? $"result = await {logicVariable}.{logicMethod.Name}();" : $"await {logicVariable}.{logicMethod.Name}();");
                }
                else
                {
                    bool isFirstParameter = true;
                    int parameterCount = requestClass.Properties.Count;

                    StringBuilder parametersBuilder = new StringBuilder();
                    foreach (var requestProperty in requestClass.Properties)
                    {
                        if (isFirstParameter)
                        {
                            parametersBuilder.Append($"{requestParameter.Name}.GetAppValueFor{requestProperty.Name}()");
                            isFirstParameter = false;
                        }
                        else
                        {
                            parametersBuilder.Append($", {requestParameter.Name}.GetAppValueFor{requestProperty.Name}()");
                        }
                    }

                    logicFormatter.AppendCodeLine(4, isServiceResult ? $"result = await {logicVariable}.{logicMethod.Name}({parametersBuilder});" : $"await {logicVariable}.{logicMethod.Name}({parametersBuilder});");
                }
                logicFormatter.AppendCodeLine(3, "}");
                logicFormatter.AppendCodeLine(3);

                logicFormatter.AppendCodeLine(3, "catch (ManagedException managed)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"Raising the handled exception to the caller of the service.\");");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, isServiceResult ? $"return ServiceResult<{returnTypeName}>.CreateError(managed);" : $"return NoDataResult.CreateError(managed);");
                logicFormatter.AppendCodeLine(3, "}");
                logicFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.CriticalLog(\"An unhandled exception occurred, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, isServiceResult ? $"return ServiceResult<{returnTypeName}>.CreateError(new UnhandledException());" : $"return NoDataResult.CreateError(new UnhandledException());");
                logicFormatter.AppendCodeLine(3, "}");
                logicFormatter.AppendCodeLine(3);
                logicFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");

                string formattedResult = null;

                if (isServiceResult)
                {
                    if (returnType.IsGrpcModelList(rpcModelProject.DefaultNamespace))
                    {
                        var grpcListType = returnType.GenericTypes.FirstOrDefault();

                        if (grpcListType == null) throw new CodeFactoryException($"Could not load the Grpc return type for the list. Cannot create the service method definition for '{serviceMethod.Name}'");
                        formattedResult = $"result.Select({grpcListType.CSharpFormatTypeName(manager)}.CreateGrpcModel).ToList()";
                    }
                    if (formattedResult == null & returnType.Namespace.StartsWith(rpcModelProject.DefaultNamespace))
                    {
                        formattedResult = $"{returnType.CSharpFormatTypeName(manager)}.CreateGrpcModel(result)";
                    }

                    if (formattedResult == null)
                    {
                        formattedResult = "result";
                    }
                }
                
                returnType.IsGrpcSupportTypeList();
                logicFormatter.AppendCodeLine(3, isServiceResult ? $"return ServiceResult<{returnTypeName}>.CreateResult({formattedResult});" : "return NoDataResult.CreateSuccess();");
                logicFormatter.AppendCodeLine(3);
                logicFormatter.AppendCodeLine(2, "}");
                logicFormatter.AppendCodeLine(2);
            }

            return logicFormatter.ReturnSource();
        }


        /// <summary>
        /// Updates a service contract definition to be in sync with the supporting logic contract.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="logicContract">Logic contract to remain in sync with.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <returns>The updated service interface.</returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task<CsInterface> UpdateServiceContractAsync(this IVsActions source, CsInterface logicContract, VsProject rpcModelProject,VsProject appModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the service contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the service contract.");
            if (rpcModelProject == null) throw new CodeFactoryException("No service model project was provided cannot update the service contract.");

            var children = await rpcModelProject.GetChildrenAsync(true, true);

            var serviceContractSource = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(s => s.SourceCode.Interfaces.Any(i => i.Name == logicContract.Name));
            
            return serviceContractSource == null ? await source.NewServiceContractAsync(logicContract, rpcModelProject,appModelProject):
                await source.UpdateServiceContractImplementationAsync(serviceContractSource,logicContract,rpcModelProject,appModelProject);
        }

        /// <summary>
        /// Creates a new service RPC contract definition. 
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="logicContract">The logic contract to be implemented.</param>
        /// <param name="rpcModelProject">The project that hosts rpc models.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <returns>Implemented interface.</returns>
        /// <exception cref="CodeFactoryException">Raised when data is missing or processing errors.</exception>
        public static async Task<CsInterface> NewServiceContractAsync(this IVsActions source, CsInterface logicContract,VsProject rpcModelProject,VsProject appModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service contract.");
            if (rpcModelProject == null) throw new CodeFactoryException("The model project was not provided cannot create the service contract.");

            var contractMethods = logicContract.Methods;

            if (!contractMethods.Any()) return null;

            string targetNamespace = rpcModelProject.DefaultNamespace;

            if (string.IsNullOrEmpty(targetNamespace)) throw new CodeFactoryException("Could not load the namespace for the module location, cannot create the service contract.");

            CsInterface result = null;
            var requestModels = new List<CsClass>();

            var serviceName = logicContract.GetClassName();

            var manualNamespaceManager = new ManualNamespaceManager(targetNamespace);

            SourceFormatter contractFormatter = new SourceFormatter();
            contractFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            contractFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            contractFormatter.AppendCodeLine(0, "using System.ServiceModel;");
            manualNamespaceManager.AddUsingStatement("System.ServiceModel");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            manualNamespaceManager.AddUsingStatement("System.Text");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            contractFormatter.AppendCodeLine(0, "using ProtoBuf.Grpc;");
            manualNamespaceManager.AddUsingStatement("ProtoBuf.Grpc");
            contractFormatter.AppendCodeLine(0, "using ProtoBuf.Grpc.Configuration;");
            manualNamespaceManager.AddUsingStatement("ProtoBuf.Grpc.Configuration");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Grpc");
            contractFormatter.AppendCodeLine(0, $"using {rpcModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(rpcModelProject.DefaultNamespace);
            contractFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            contractFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
            contractFormatter.AppendCodeLine(0, "{");
            contractFormatter.AppendCodeLine(1, $"[Service()]");
            contractFormatter.AppendCodeLine(1, $"public interface {logicContract.Name}");
            contractFormatter.AppendCodeLine(1, "{");

            var manager = manualNamespaceManager.BuildNamespaceManager();

            foreach (var contractMethod in contractMethods)
            {
                string requestTypeName = null;
                CsClass requestModel = null;

                bool hasParameters = contractMethod.Parameters.Any();

                if (hasParameters)
                {
                    requestTypeName = contractMethod.GetServiceRequestModelName(serviceName);

                    requestModel = requestModels.FirstOrDefault(x => x.Name == requestTypeName);

                    if (requestModel == null)
                    {
                        requestModel = await source.GetGrpcRequestModelAsync(rpcModelProject, appModelProject,contractMethod, serviceName);
                        if (requestModel == null) throw new CodeFactoryException($"could not load the request type to support the method '{contractMethod.Name}', cannot create the service contract.");
                        requestModels.Add(requestModel);
                    }
                    
                }

                var serviceResult = await source.GetServiceRpcReturnTypeSyntaxAsync(rpcModelProject,appModelProject, contractMethod,manager);

                if (serviceResult == null) throw new CodeFactoryException($"Could not generate the service return type for the methiod '{contractMethod.Name}', cannot create the service contract");

                var serviceMethodName = contractMethod.GetServiceMethodRpcName(serviceName);

                if (serviceMethodName == null) throw new CodeFactoryException($"Could not generate the service method name for the method '{contractMethod.Name}',cannot create the service contract");

                contractFormatter.AppendCodeLine(2, "/// <summary>");
                contractFormatter.AppendCodeLine(2, $"/// Grpc service call that supports the logic contract of <see cref=\"{logicContract.Namespace}.{logicContract.Name}\"/> ");
                contractFormatter.AppendCodeLine(2, "/// </summary>");
                if(hasParameters) contractFormatter.AppendCodeLine(2, "/// <param name=\"request\">The request data to be processed service request.</param>");
                contractFormatter.AppendCodeLine(2, "/// <param name=\"context\">The calling context from the environment.This is an optional parameter that is populated by the environment, do not provide a value.</param>");
                contractFormatter.AppendCodeLine(2, "[Operation]");
                contractFormatter.AppendCodeLine(2, hasParameters ? $"{serviceResult} {serviceMethodName}({requestModel.CSharpFormatBaseTypeName(manager)} request, CallContext context = default);" : $"{serviceResult} {serviceMethodName}(CallContext context = default);");
                contractFormatter.AppendCodeLine(2);
            }

            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var contractDocument = await rpcModelProject.AddDocumentAsync($"{logicContract.Name}.cs", contractFormatter.ReturnSource());

            var contractSource = await contractDocument.GetCSharpSourceModelAsync();

            if (contractSource == null) throw new CodeFactoryException($"Error loading the created contract for the logic contract of '{logicContract.Name}'");

            result = contractSource.Interfaces.FirstOrDefault();

            if(result == null) throw new CodeFactoryException($"Error loading the created contract for the logic contract of '{logicContract.Name}'");

            return result;

        }

        /// <summary>
        /// Updates an existing Grpc service contract.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="serviceContractSource">Source contract document to update.</param>
        /// <param name="logicContract">Logic contract to use to update the service contract.</param>
        /// <param name="rpcModelProject">Grpc models project.</param>
        /// <param name="appModelProject">Project that hosts application model projects.</param>
        /// <returns>The update service contract interface.</returns>
        public static async Task<CsInterface> UpdateServiceContractImplementationAsync(this IVsActions source, VsCSharpSource serviceContractSource, CsInterface logicContract, VsProject rpcModelProject, VsProject appModelProject )
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the service contract.");
            if (serviceContractSource == null) throw new CodeFactoryException("No source service contract was provided cannot update the service contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the service contract.");
            if (rpcModelProject == null) throw new CodeFactoryException("The Grpc model project was not provided cannot update the service contract.");
            if (appModelProject == null) throw new CodeFactoryException("The app model project was not provided cannot update the service contract.");

            var serviceContract = serviceContractSource.SourceCode.Interfaces.FirstOrDefault();
            if(serviceContract == null) throw new CodeFactoryException("No source service contract was provided cannot update the service contract.");


            var requestModels = new List<CsClass>();

            var serviceName = logicContract.GetClassName();

            var contractMethods = logicContract.Methods;

            var serviceMethods = serviceContract.Methods;

            CsSource currentServiceSource = serviceContractSource.SourceCode;

            foreach (var serviceMethod in serviceMethods)
            {
                if (currentServiceSource.GetModel(serviceMethod.LookupPath) is CsMethod currentMethod) currentServiceSource = await currentMethod.DeleteAsync();
            }

            serviceContract = currentServiceSource.Interfaces.FirstOrDefault();

            if (serviceContract == null) throw new CodeFactoryException("No source service contract was provided cannot update the service contract.");

            SourceFormatter contractFormatter = new SourceFormatter();

            foreach (var contractMethod in contractMethods)
            {
                string requestTypeName = null;
                CsClass requestModel = null;

                bool hasParameters = contractMethod.Parameters.Any();

                if (hasParameters)
                {
                    requestTypeName = contractMethod.GetServiceRequestModelName(serviceName);

                    requestModel = requestModels.FirstOrDefault(x => x.Name == requestTypeName);

                    if (requestModel == null)
                    {
                        requestModel = await source.GetGrpcRequestModelAsync(rpcModelProject,appModelProject, contractMethod, serviceName);
                        if (requestModel == null) throw new CodeFactoryException($"could not load the request type to support the method '{contractMethod.Name}', cannot create the service contract.");
                        requestModels.Add(requestModel);
                    }
                }

                var manager = new NamespaceManager(serviceContractSource.SourceCode.NamespaceReferences, serviceContractSource.SourceCode.Namespaces.FirstOrDefault()?.Name);

                var serviceResult = await source.GetServiceRpcReturnTypeSyntaxAsync(rpcModelProject,appModelProject, contractMethod,manager);

                if (serviceResult == null) throw new CodeFactoryException($"Could not generate the service return type for the method '{contractMethod.Name}', cannot create the service contract");

                var serviceMethodName = contractMethod.GetServiceMethodRpcName(serviceName);

                if (serviceMethodName == null) throw new CodeFactoryException($"Could not generate the service method name for the method '{contractMethod.Name}',cannot create the service contract");

                contractFormatter.AppendCodeLine(2, "/// <summary>");
                contractFormatter.AppendCodeLine(2, $"/// Grpc service call that supports the logic contract of <see cref=\"{logicContract.Namespace}.{logicContract.Name}\"/> ");
                contractFormatter.AppendCodeLine(2, "/// </summary>");
                if (hasParameters) contractFormatter.AppendCodeLine(2, "/// <param name=\"request\">The request data to be processed service request.</param>");
                contractFormatter.AppendCodeLine(2, "/// <param name=\"context\">The calling context from the environment.This is an optional parameter that is populated by the environment, do not provide a value.</param>");
                contractFormatter.AppendCodeLine(2, "[Operation]");
                contractFormatter.AppendCodeLine(2, hasParameters ? $"{serviceResult} {serviceMethodName}({requestModel.Namespace}.{requestModel.Name} request, ProtoBuf.Grpc.CallContext context = default);" : $"{serviceResult} {serviceMethodName}(ProtoBuf.Grpc.CallContext context = default);");
                contractFormatter.AppendCodeLine(2);
            }

            currentServiceSource = await serviceContract.AddToEndAsync(contractFormatter.ReturnSource());

            CsInterface result = currentServiceSource.Interfaces.FirstOrDefault();

            if (result == null) throw new CodeFactoryException($"Error loading the updated contract for the logic contract of '{logicContract.Name}'");

            return result;
        }

        /// <summary>
        /// Formats the C# syntax for a service return type from a Grpc service method.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="rpcModelProject">The grpc project that hosts the models.</param>
        /// <param name="appModelProject">The project that hosts application models.</param>
        /// <param name="contractMethod">The contract method to implemented.</param>
        /// <param name="manager">The namespace manager to reduce signature size.</param>
        /// <returns>Formatted return type syntax.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing, or processing errors occurred. </exception>
        public static async Task<string> GetServiceRpcReturnTypeSyntaxAsync(this IVsActions source,VsProject rpcModelProject,VsProject appModelProject, CsMethod contractMethod,NamespaceManager manager)
        {
            if (source == null) throw new CodeFactoryException("No CodeFactory automation was provided, cannot define the return type syntax.");
            if (rpcModelProject == null) throw new CodeFactoryException("The grpc models project was not provided, cannot define the return type syntax.");
            if (contractMethod == null) throw new CodeFactoryException("No contract method was provided, cannot define the return type syntax.");

            if (contractMethod.IsVoid) return "Task<NoDataResult>";

            var returnType = contractMethod.ReturnType;

            if (returnType.IsGrpcSupportedType()) return $"Task<ServiceResult<{returnType.CSharpFormatTypeName(manager)}>>";

            if (returnType == null) throw new CodeFactoryException($"Cannot get the return type for the contract method '{contractMethod.Name}', cannot define the return type syntax.");

            CsClass appModel = null;
            ModelProcessResult<CsClass> grpcModel = null;

            if (returnType.Namespace == "System.Threading.Tasks" & returnType.Name == "Task")
            {
                if (!returnType.IsGeneric) return "Task<NoDataResult>";

                var taskGeneric = returnType.GenericTypes.FirstOrDefault();

                if (taskGeneric == null) throw new CodeFactoryException($"Cannot get the target type information for a task generic return type for contract  method '{contractMethod.Name}', cannot define the return type syntax.");

                if (taskGeneric.Namespace == "System.Collections.Generic" & taskGeneric.Name == "List")
                {
                    

                    var listGeneric = taskGeneric.GenericTypes.FirstOrDefault();
                    if (listGeneric == null) throw new CodeFactoryException($"Cannot get the generic list type information for a task list return type for contact method '{contractMethod.Name}', cannot define the return type syntax.");
                    
                    if (taskGeneric.IsGrpcSupportTypeList()) return $"Task<ServiceResult<List<{listGeneric.CSharpFormatTypeName(manager)}>>>";

                    if (!taskGeneric.IsGrpcModelList(appModelProject.DefaultNamespace)) throw new CodeFactoryException($"The list type of '{listGeneric.FormatCSharpFullTypeName()}' is not supported for Grpc services. The contract method '{contractMethod.Name}' cannot be returned from a service request.");

                    appModel = listGeneric.GetClassModel();

                    grpcModel = await source.GetGrpcModelAsync(rpcModelProject,appModelProject, appModel, null);
                    if (grpcModel.Result == null) throw new CodeFactoryException($"Cannot generate the grpc model for the application model for contact method '{contractMethod.Name}',cannot define the return type syntax.");
                    return $"Task<ServiceResult<List<{grpcModel.Result.CSharpFormatBaseTypeName(manager)}>>>";
                }

                if (taskGeneric.IsGrpcSupportedType()) return $"Task<ServiceResult<{taskGeneric.CSharpFormatTypeName(manager)}>>";

                if (!taskGeneric.IsModel(appModelProject.DefaultNamespace)) throw new CodeFactoryException($"The type of '{taskGeneric.CSharpFormatTypeName(manager)}' is not supported for Grpc services. The contract method '{contractMethod.Name}' cannot be returned from a service request.");

                appModel = taskGeneric.GetClassModel();
                grpcModel = await source.GetGrpcModelAsync(rpcModelProject,appModelProject, appModel, null);
                if (grpcModel.Result == null) throw new CodeFactoryException($"Cannot generate the grpc model for the application model for contact method '{contractMethod.Name}',cannot define the return type syntax.");
                return $"Task<ServiceResult<{grpcModel.Result.CSharpFormatBaseTypeName(manager)}>>";

            }

            if (returnType.Namespace == "System.Collections.Generic" & returnType.Name == "List")
            {
                var listGeneric = returnType.GenericTypes.FirstOrDefault();
                if (listGeneric == null) throw new CodeFactoryException($"Cannot get the generic list type information for the list return type for contact method '{contractMethod.Name}', cannot define the return type syntax.");

                if (returnType.IsGrpcSupportTypeList()) return $"Task<ServiceResult<List<{listGeneric.CSharpFormatTypeName(manager)}>>>";

                if (!returnType.IsGrpcModelList(appModelProject.DefaultNamespace)) throw new CodeFactoryException($"The list type of '{listGeneric.CSharpFormatTypeName(manager)}' is not supported for Grpc services. The contract method '{contractMethod.Name}' cannot be returned from a service request.");

                appModel = listGeneric.GetClassModel();
                grpcModel = await source.GetGrpcModelAsync(rpcModelProject,appModelProject, appModel, null);
                if (grpcModel.Result == null) throw new CodeFactoryException($"Cannot generate the grpc model for the application model for contact method '{contractMethod.Name}',cannot define the return type syntax.");
                return $"Task<ServiceResult<List<{grpcModel.Result.CSharpFormatBaseTypeName(manager)}>>>";
            }

            if (!returnType.IsModel(appModelProject.DefaultNamespace)) throw new CodeFactoryException($"The type of '{returnType.CSharpFormatTypeName(manager)}' is not supported for Grpc services. The contract method '{contractMethod.Name}' cannot be returned from a service request.");

            appModel = returnType.GetClassModel();

            grpcModel = await source.GetGrpcModelAsync(rpcModelProject,appModelProject, appModel, null);

            if (grpcModel.Result == null) throw new CodeFactoryException($"Cannot generate the grpc model for the application model for contact method '{contractMethod.Name}',cannot define the return type syntax.");
            
            return $"Task<ServiceResult<{grpcModel.Result.CSharpFormatBaseTypeName(manager)}>>";
        }

        /// <summary>
        /// Generates the name of a service request method from the hosting contract method.
        /// </summary>
        /// <param name="source">The contract method to generate the service method from.</param>
        /// <param name="serviceName">The name of the service to generate.</param>
        /// <returns>Formatted service method name.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is not provided, or a processing error occurs.</exception>
        public static string GetServiceMethodRpcName(this CsMethod source, string serviceName)
        {
            if (source == null) throw new CodeFactoryException("The source method was not provided, cannot generate the service method name for rpc.");
            if (string.IsNullOrEmpty(serviceName)) throw new CodeFactoryException("The service name was not provided cannot generate the service method name for rpc.");

            var requestName = source.GetServiceRequestModelName(serviceName);

            if (requestName == null) return source.Name;

            var requestBaseName =  requestName.Replace(serviceName,"").Replace("Request","").Trim();
            string result = null;
            if (source.Name.EndsWith("Async")) result = source.Name.Replace("Async", $"By{requestBaseName}Async");
            else result = $"{source.Name}By{requestBaseName}";

            return result;
        }
    }
}