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
    /// Extensions and functionality support the automation of logic.
    /// </summary>
    public static class LogicExtensions
    {
        /// <summary>
        /// Updates a Grpc service implementation for the target logic interface definition.
        /// </summary>
        /// <param name="source">CodeFactory Automation</param>
        /// <param name="logicSource">The source document that contains the logic interface to be updated.</param>
        /// <exception cref="CodeFactoryException">Raised if data is missing or a logic error occurs.</exception>
        public static async Task UpdateGrpcServiceImplementationAsync(this IVsActions source, VsCSharpSource logicSource,VsProject appModelProject, VsProject rpcModelProject, VsProject abstractionProject, VsProject rpcProject, VsProject logicProject, string abstractionSuffix)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "CodeFactory actions was not supplied cannot update service implementation.");
            if (logicSource == null)
                throw new CodeFactoryException("No source logic was provided cannot update service implementation");
            

            var logicContract = logicSource?.SourceCode?.Interfaces.FirstOrDefault();
            if (logicContract == null)
                throw new CodeFactoryException(
                    "No logic interface contract was provided cannot update the service implementation");

            VsProject serviceProject = rpcProject;

            if (serviceProject == null)
                throw new CodeFactoryException(
                    "Could not load the service rpc module project, cannot update the service implementation.");

            var serviceModelProject = rpcModelProject;
            if (serviceModelProject == null)
                throw new CodeFactoryException(
                    "Could not load the service rpc model project, cannot update the service implementation.");

            
            if (abstractionProject == null)
                throw new CodeFactoryException(
                    "Could not load the web abstraction project, cannot update the service implementation.");

            var serviceContract =
                await source.UpdateServiceContractAsync(logicContract, serviceModelProject,appModelProject);

            if (serviceContract == null)
                throw new CodeFactoryException(
                    "The service contract was not updated, cannot update the service implementation.");

            var serviceImplementation = await source.UpdateGrpcServiceAsync(serviceContract, logicContract,appModelProject,rpcModelProject,rpcProject,logicProject);

            if (serviceImplementation == null)
                throw new CodeFactoryException("Could not update the service implementation.");

            await source.UpdateWebAbstraction(logicContract, serviceContract,appModelProject,rpcModelProject,abstractionProject,abstractionSuffix);

        }

        /// <summary>
        /// Updates a web abstraction implementation, by building upon a service and logic implementation.
        /// </summary>
        /// <param name="source">CodeFactory Automation</param>
        /// <param name="logicContract">The logic contract to support for the abstraction.</param>
        /// <param name="serviceContract">The service implementation to call.</param>
        /// <param name="targetModule">The target service module to call with the abstraction.</param>
        public static async Task UpdateWebAbstraction(this IVsActions source, CsInterface logicContract, CsInterface serviceContract, VsProject appModelProject, VsProject rpcModelProject, VsProject abstractionProject,string abstractionSuffix)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the web abstraction.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create cannot update the web abstraction.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided  cannot update the web abstraction.");


            var abstractProject = abstractionProject;
            if (abstractProject == null) throw new CodeFactoryException("Could not load the web abstraction project cannot update the web abstraction.");


            var abstractionContractSource = await abstractProject.FindCSharpSourceByInterfaceNameAsync($"{logicContract.Name}{abstractionSuffix}");

            var abstractionContract = abstractionContractSource != null ? await source.UpdateAbstractionContractAsync(abstractionContractSource, logicContract) : await source.NewAbstractionContractAsync(logicContract,appModelProject,rpcModelProject,abstractionProject,abstractionSuffix);

            if (abstractionContract == null) throw new CodeFactoryException("Could note load the abstraction contract interface cannot update the web abstraction.");

            var abstractClassName =$"Rpc{abstractionContract.GetClassName()}";

            var abstractClassSource = await abstractProject.FindCSharpSourceByClassNameAsync(abstractClassName);

            var abstraction = abstractClassSource == null ? await source.NewAbstractionAsync(abstractionContract, serviceContract,appModelProject,rpcModelProject,abstractionProject) : await source.UpdateAbstractionAsync(abstractClassSource, abstractionContract, serviceContract,rpcModelProject);

            if (abstraction == null) throw new CodeFactoryException($"Could not load the updated abstraction '{abstractClassName}' cannot confirm if the update was completed.");
        }

        /// <summary>
        /// Creates a new web abstraction from scratch.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="abstractContract">The abstraction contract to implement.</param>
        /// <param name="serviceContract">The service contract to be used with the abstractions.</param>
        /// <param name="targetModule">The target service model for service calls.</param>
        /// <returns>The implemented abstraction class.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or a logic error occurred.</exception>
        public static async Task<CsClass> NewAbstractionAsync(this IVsActions source, CsInterface abstractContract, CsInterface serviceContract,VsProject appModelProject, VsProject rpcModelProject, VsProject abstractionProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the web abstraction.");
            if (abstractContract == null) throw new CodeFactoryException("No logic contract was provided cannot create cannot update the web abstraction.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided  cannot update the web abstraction.");


            var abstractProject = abstractionProject;
            if (abstractProject == null) throw new CodeFactoryException("Could not load the web abstraction project cannot create the web abstraction contract.");


            var abstractionClassName = abstractContract.GetClassName();

            var abstractionNamespace = abstractProject.DefaultNamespace;
            if (string.IsNullOrEmpty(abstractionNamespace)) throw new CodeFactoryException("Could not create the namespace for the web abstraction contract.");

            await source.CheckAbstractionServiceUrlAsync(abstractionProject);

            var serviceUrlParameter = $"RpcServiceUrl";

            var abstractionFormatter = new SourceFormatter();

            var manualNamespaceManager = new ManualNamespaceManager(abstractionNamespace);
            
            abstractionFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System");
            abstractionFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            abstractionFormatter.AppendCodeLine(0, "using System.Text;");
            manualNamespaceManager.AddUsingStatement("System.Text");
            abstractionFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            abstractionFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            abstractionFormatter.AppendCodeLine(0, "using System.Net.Http;");
            manualNamespaceManager.AddUsingStatement("System.Net.Http");
            abstractionFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            manualNamespaceManager.AddUsingStatement("Microsoft.Extensions.Logging");
            abstractionFormatter.AppendCodeLine(0, "using Grpc.Net.Client;");
            manualNamespaceManager.AddUsingStatement("Grpc.Net.Client");
            abstractionFormatter.AppendCodeLine(0, "using ProtoBuf.Grpc.Client;");
            manualNamespaceManager.AddUsingStatement("ProtoBuf.Grpc.Client");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Service");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Grpc");
            abstractionFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            abstractionFormatter.AppendCodeLine(0, $"using {rpcModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(rpcModelProject.DefaultNamespace);

            
            if (serviceContract.Namespace != rpcModelProject.DefaultNamespace)
            {
                abstractionFormatter.AppendCodeLine(0, $"using {serviceContract.Namespace};");
                manualNamespaceManager.AddUsingStatement(serviceContract.Namespace);
            }
            
            if (abstractionNamespace != abstractProject.DefaultNamespace)
            {
                abstractionFormatter.AppendCodeLine(0, $"using {abstractProject.DefaultNamespace};");
                manualNamespaceManager.AddUsingStatement(abstractProject.DefaultNamespace);
            }

            abstractionFormatter.AppendCodeLine(0, $"namespace {abstractionNamespace}");
            abstractionFormatter.AppendCodeLine(0, "{");

            abstractionFormatter.AppendCodeLine(1, "/// <summary>");
            abstractionFormatter.AppendCodeLine(1, $"/// Web abstraction implementation for abstraction contract '{abstractContract.Name}'");
            abstractionFormatter.AppendCodeLine(1, "/// </summary>");
            abstractionFormatter.AppendCodeLine(1, $"public class {abstractionClassName}:{abstractContract.Name}");
            abstractionFormatter.AppendCodeLine(1, "{");
            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Url for the service being accessed by this abstraction.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, $"private readonly ServiceUrl _{serviceUrlParameter};");
            abstractionFormatter.AppendCodeLine(2);

            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Grpc channel to use for service calls.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, "private readonly GrpcChannel _channel;");
            abstractionFormatter.AppendCodeLine(2);

            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Logger used for the class.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            abstractionFormatter.AppendCodeLine(2);

            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Creates a instance of the web abstraction.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger that supports this abstraction.</param>");
            abstractionFormatter.AppendCodeLine(2, "/// <param name=\"url\">service url</param>");
            abstractionFormatter.AppendCodeLine(2, $"public {abstractionClassName}(ILogger<{abstractionClassName}> logger, RpcServiceUrl url)");
            abstractionFormatter.AppendCodeLine(2, "{");

            abstractionFormatter.AppendCodeLine(3, "_logger = logger;");
            abstractionFormatter.AppendCodeLine(3, $"_{serviceUrlParameter} = url;");
            
            abstractionFormatter.AppendCodeLine(3, $"_channel = GrpcChannel.ForAddress(_{serviceUrlParameter}.Url, new GrpcChannelOptions");
            abstractionFormatter.AppendCodeLine(3, "{");
            abstractionFormatter.AppendCodeLine(4, "HttpHandler = new SocketsHttpHandler");
            abstractionFormatter.AppendCodeLine(4, "{");
            abstractionFormatter.AppendCodeLine(5, "EnableMultipleHttp2Connections = true");
            abstractionFormatter.AppendCodeLine(4, "}");
            abstractionFormatter.AppendCodeLine(3, "});");

            abstractionFormatter.AppendCodeLine(2, "}");
            abstractionFormatter.AppendCodeLine(2);

            var contractMethods = abstractContract.Methods.ToList();

            if (contractMethods.Any())
            {
                var serviceName = serviceContract.GetClassName();
                var contentMethods = new List<Tuple<CsMethod, CsMethod>>();
                foreach (var contractMethod in contractMethods)
                {
                    var serviceMethodName = contractMethod.GetServiceMethodRpcName(serviceName);

                    var serviceMethod = serviceContract.Methods.FirstOrDefault(m => m.Name == serviceMethodName);

                    if (serviceMethod != null) contentMethods.Add(new Tuple<CsMethod, CsMethod>(contractMethod, serviceMethod));
                }

                if (contentMethods.Any())
                {
                    var namespaceManager = manualNamespaceManager.BuildNamespaceManager();
                    var content = source.BuildAbstractionContent(contentMethods,serviceContract,rpcModelProject, namespaceManager);

                    if (!string.IsNullOrEmpty(content)) abstractionFormatter.AppendCode(content);
                }
            }

            abstractionFormatter.AppendCodeLine(1, "}");
            abstractionFormatter.AppendCodeLine(0, "}");

            var abstractionDocument = await abstractProject.AddDocumentAsync($"{abstractionClassName}.cs", abstractionFormatter.ReturnSource());
            if (abstractionDocument == null) throw new CodeFactoryException($"Could not load the document for abstraction {abstractionClassName}");

            var abstractionSource = await abstractionDocument.GetCSharpSourceModelAsync();
            if (abstractionSource == null) throw new CodeFactoryException($"Could not load the source code for abstraction {abstractionClassName}");

            CsClass abstraction = abstractionSource.Classes.FirstOrDefault();

            if (abstraction == null) throw new CodeFactoryException($"Could not load the contract class for abstraction {abstractionClassName}");

            return abstraction;

        }

        /// <summary>
        /// Updates an existing web abstraction.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="abstractSource">The source code for the abstraction to update.</param>
        /// <param name="abstractContract">The abstraction contract to make sure is implemented./param>
        /// <param name="serviceContract">The service contract that is called by the abstraction.</param>
        /// <returns>class definition that has been implemented.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or a logic error occurred.</exception>
        public static async Task<CsClass> UpdateAbstractionAsync(this IVsActions source, VsCSharpSource abstractSource, CsInterface abstractContract,CsInterface serviceContract,VsProject rpcModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the web abstraction.");
            if (abstractSource == null) throw new CodeFactoryException("No abstraction source was provided cannot update the web abstraction.");
            if (abstractContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the web abstraction.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided cannot update the web abstraction.");


            var abstractClass = abstractSource.SourceCode.Classes.FirstOrDefault();

            if (abstractClass == null) throw new CodeFactoryException($"Cannot load the web abstraction class cannot update the web abstraction.");

            var classComparison =  abstractClass.FormatCSharpComparisonMembers(CodeFactory.DotNet.MemberComparisonType.Security);

            var contractComparison = abstractContract.FormatCSharpComparisonMembers(CodeFactory.DotNet.MemberComparisonType.Security);

            var contractMethods = new List<CsMethod>();

            contractMethods = contractComparison.Where(c=> !classComparison.Any(d => d.Key == c.Key)).Where(r => r.Value.ModelType == CsModelType.Method).Select(r => r.Value as CsMethod).ToList();

            if (contractMethods.Any())
            {
                var serviceName = serviceContract.GetClassName();
                var contentMethods = new List<Tuple<CsMethod, CsMethod>>();
                foreach (var contractMethod in contractMethods)
                {
                    var serviceMethodName = contractMethod.GetServiceMethodRpcName(serviceName);

                    var serviceMethod = serviceContract.Methods.FirstOrDefault(m => m.Name == serviceMethodName);

                    if (serviceMethod != null) contentMethods.Add(new Tuple<CsMethod, CsMethod>(contractMethod, serviceMethod));
                }

                if (contentMethods.Any())
                {
                    var namespaceManager = new NamespaceManager(abstractSource.SourceCode.NamespaceReferences, abstractClass.Namespace);
                    var content = source.BuildAbstractionContent(contentMethods, serviceContract,rpcModelProject, namespaceManager);

                    var updatedSource = await abstractClass.AddToEndAsync(content);

                    if (updatedSource == null) throw new CodeFactoryException("Could not load the updated source code for the abstraction class.");

                    abstractClass = updatedSource.Classes.FirstOrDefault();

                    if (abstractClass == null) throw new CodeFactoryException("Could not load the updated source code class for the abstraction");

                }
                else return abstractClass;
            }

            return abstractClass;
        }

        /// <summary>
        /// Create the web abstraction method calls.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="abstractionMethods">List of the abstraction methods and service method to consume.</param>
        /// <param name="serviceContract">The service contract that is being used with the abstraction calls.</param>
        /// <returns>Formatted methods.</returns>
        /// <exception cref="CodeFactoryException">If data is missing or a logic error occurred.</exception>
        public static  string BuildAbstractionContent(this IVsActions source, List<Tuple<CsMethod, CsMethod>> abstractionMethods,CsInterface serviceContract,VsProject rpcModelProject, NamespaceManager manager)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create content for a web abstraction.");
            if (abstractionMethods == null) throw new CodeFactoryException("No abstraction methods to implement were provided, cannot create content for a web abstraction.");
            if (serviceContract == null) throw new CodeFactoryException("No service contract was provided, cannot create content for a web abstraction");
            if (!abstractionMethods.Any()) throw new CodeFactoryException("No abstraction methods to implement were provided, cannot create content for a web abstraction.");

            var contentFormatter = new SourceFormatter();

            foreach (var abstractionMethod in abstractionMethods)
            {
                var contractMethod = abstractionMethod.Item1;
                var serviceMethod = abstractionMethod.Item2;

                

                var contractReturnType = contractMethod.ReturnType;


                bool hasNoReturnType = (contractMethod.IsVoid) | (contractReturnType.Namespace == "System.Threading.Tasks" & contractReturnType.Name == "Task" & !contractReturnType.HasStrongTypesInGenerics);

                CsType returnType = null;
                CsType serviceReturnType = null;

                if (!hasNoReturnType)
                {
                    if (contractReturnType.Namespace == "System.Threading.Tasks" & contractReturnType.Name == "Task" & contractReturnType.HasStrongTypesInGenerics) returnType = contractReturnType.GenericTypes.FirstOrDefault();
                    else returnType = contractReturnType;

                    if (returnType == null) throw new CodeFactoryException($"Could not load the return type information for the abstraction method '{contractMethod.Name}'");

                    var serviceResultType = serviceMethod.ReturnType.GenericTypes.FirstOrDefault();
                    if (serviceResultType.Namespace == "CommonDeliveryFramework.Grpc" & serviceResultType.Name == "ServiceResult") serviceReturnType = serviceResultType.GenericTypes.FirstOrDefault();
                    else throw new CodeFactoryException($"The service method '{serviceMethod.Name}' return type of '{serviceMethod.ReturnType.CSharpFormatTypeName(manager)}' is not supported for abstraction automatic generation.");
                    if (serviceReturnType == null) throw new CodeFactoryException($"Could not load the return type information for the service method '{serviceMethod.Name}'");
                }

                if (contractMethod.HasDocumentation)
                {
                    var doc = contractMethod.CSharpFormatXmlDocumentationEnumerator();

                    foreach (var docLine in doc)
                    {
                        contentFormatter.AppendCodeLine(2, docLine);
                    }
                }

                if (contractMethod.HasAttributes)
                {
                    foreach (var attributeData in contractMethod.Attributes)
                    {
                        contentFormatter.AppendCodeLine(2, attributeData.CSharpFormatAttributeSignature(manager));
                    }
                }

                contentFormatter.AppendCodeLine(2, $"{contractMethod.CSharpFormatStandardMethodSignatureWithAsync(manager)}");
                contentFormatter.AppendCodeLine(2, "{");

                contentFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");


                if (contractMethod.HasParameters)
                {

                    foreach (ICsParameter paramData in contractMethod.Parameters)
                    {
                        //If the parameter has a default value then continue will not bounds check parameters with a default value.
                        if (paramData.HasDefaultValue) continue;

                        //If the parameter is a string type add the following bounds check
                        if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if(string.IsNullOrEmpty({paramData.Name}))");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4, $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }

                        // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                        if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if({paramData.Name} == null)");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4, $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }
                    }
                }

                if (!hasNoReturnType)
                {
                    contentFormatter.AppendCodeLine(3, returnType.IsClass ? $"{returnType.CSharpFormatTypeName(manager)} result = null; " : $"{returnType.CSharpFormatTypeName(manager)} result;");
                    contentFormatter.AppendCodeLine(3);
                }

                contentFormatter.AppendCodeLine(3, "try");
                contentFormatter.AppendCodeLine(3, "{");

                contentFormatter.AppendCodeLine(4, $"var client = _channel.CreateGrpcService<{serviceContract.Name}>();");

                if (serviceMethod.Parameters.Count == 2)
                {
                    var requestTypeParameter = serviceMethod.Parameters.FirstOrDefault();

                    if (requestTypeParameter == null) throw new CodeFactoryException($"Could not load the service request type for service method '{serviceMethod.Name}'");



                    var requestClass = requestTypeParameter.ParameterType.GetClassModel();

                    if (requestClass == null) throw new CodeFactoryException($"Could not load the service request class definition for service method '{serviceMethod.Name}'");


                    bool isFirstParameter = true;

                    StringBuilder constructionParameters = new StringBuilder();

                    foreach (var parameter in contractMethod.Parameters)
                    {
                        if (isFirstParameter)
                        {
                            constructionParameters.Append(parameter.Name);
                            isFirstParameter = false;
                        }
                        else
                        {
                            constructionParameters.Append($", {parameter.Name}");
                        }
                    }

                    contentFormatter.AppendCodeLine(4, $"var request = {requestClass.CSharpFormatBaseTypeName(manager)}.CreateGrpcModel({constructionParameters});");
                    contentFormatter.AppendCodeLine(4);
                    contentFormatter.AppendCodeLine(4,$"var rpcResult = await client.{serviceMethod.Name}(request);");
                    contentFormatter.AppendCodeLine(4);

                }
                else
                {
                    contentFormatter.AppendCodeLine(4, $"var rpcResult = await client.{serviceMethod.Name}();");
                    contentFormatter.AppendCodeLine(4);
                }

                contentFormatter.AppendCodeLine(4, "rpcResult.RaiseException();");

                if (!hasNoReturnType)
                { 
                    string resultStatement = null;

                    if (serviceReturnType.IsGrpcModelList(rpcModelProject.DefaultNamespace))
                    {
                        resultStatement = $"result = rpcResult.Result.Select(d=> d.CreateAppModel()).ToList();";
                    }
                    if (resultStatement == null & serviceReturnType.Namespace.StartsWith(rpcModelProject.DefaultNamespace))
                    {
                        resultStatement = $"result = rpcResult.Result.CreateAppModel();";
                    }
                    if(resultStatement == null) resultStatement = "result = rpcResult.Result;";

                    contentFormatter.AppendCodeLine(4, resultStatement);
                    contentFormatter.AppendCodeLine(4);
                }

                contentFormatter.AppendCodeLine(3, "}");

                contentFormatter.AppendCodeLine(3, "catch (ManagedException)");
                contentFormatter.AppendCodeLine(3, "{");
                contentFormatter.AppendCodeLine(4, "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
                contentFormatter.AppendCodeLine(4, "throw;");
                contentFormatter.AppendCodeLine(3, "}");

                contentFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
                contentFormatter.AppendCodeLine(3, "{");
                contentFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"An unhandled exception occurred, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                contentFormatter.AppendCodeLine(4, "throw new UnhandledException();");
                contentFormatter.AppendCodeLine(3, "}");
                contentFormatter.AppendCodeLine(3);
                contentFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
                contentFormatter.AppendCodeLine(3);

                if(!hasNoReturnType) contentFormatter.AppendCodeLine(3,"return result;");

                contentFormatter.AppendCodeLine(2, "}");
                contentFormatter.AppendCodeLine(2);

            }

            return contentFormatter.ReturnSource();
        }

                /// <summary>
        /// Creates a new web abstraction contract from a target supporting logic contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="logicContract">The logic contract to clone into a web abstraction contract.</param>
        /// <param name="targetModule">The target module the web abstraction will support.</param>
        /// <returns>The implemented interface.</returns>
        public static async Task<CsInterface> NewAbstractionContractAsync(this IVsActions source, CsInterface logicContract,VsProject appModelProject, VsProject rpcModelProject, VsProject abstractionProject,string abstractionSuffix)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the web abstraction contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the web abstraction contract.");


            var abstractProject = abstractionProject;


            var abstractionName = $"IRpc{logicContract.GetClassName()}{abstractionSuffix}";

            var abstractionNamespace = abstractProject.DefaultNamespace;
            if (string.IsNullOrEmpty(abstractionNamespace)) throw new CodeFactoryException("Could not create the namespace for the web abstraction contract.");

            var contractFormatter = new SourceFormatter();
            ManualNamespaceManager  manualNamespaceManager = new ManualNamespaceManager(abstractionNamespace);

            contractFormatter.AppendCodeLine(0, "using System;");
            manualNamespaceManager.AddUsingStatement("System");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            manualNamespaceManager.AddUsingStatement("System.Collections.Generic");
            contractFormatter.AppendCodeLine(0, "using System.Linq;");
            manualNamespaceManager.AddUsingStatement("System.Linq");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            manualNamespaceManager.AddUsingStatement("System.Text");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            manualNamespaceManager.AddUsingStatement("System.Threading.Tasks");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Grpc;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Grpc");
            contractFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            contractFormatter.AppendCodeLine(0, $"using {rpcModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement(rpcModelProject.DefaultNamespace);

            var namespaceManager = manualNamespaceManager.BuildNamespaceManager();

            contractFormatter.AppendCodeLine(0, $"namespace {abstractionNamespace}");
            contractFormatter.AppendCodeLine(0, "{");

            contractFormatter.AppendCodeLine(1, "/// <summary>");
            contractFormatter.AppendCodeLine(1, $"/// Web abstraction that accesses application logic for '{logicContract.GetClassName()}'");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1, $"public interface {abstractionName}");
            contractFormatter.AppendCodeLine(1, "{");

            var contractMethods = logicContract.Methods.ToList();

            if (contractMethods.Any())
            {
                var content = source.BuildAbstractionContractContent(contractMethods,namespaceManager);

                if (!string.IsNullOrEmpty(content)) contractFormatter.AppendCode(content);
            }

            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var contractDocument = await abstractProject.AddDocumentAsync($"{abstractionName}.cs", contractFormatter.ReturnSource());
            if (contractDocument == null) throw new CodeFactoryException($"Could not load the document for contract abstraction {abstractionName}");

            var contractSource = await contractDocument.GetCSharpSourceModelAsync();
            if (contractSource == null) throw new CodeFactoryException($"Could not load the source code for contract abstraction {abstractionName}");

            CsInterface contract = contractSource.Interfaces.FirstOrDefault();

            if (contract == null) throw new CodeFactoryException($"Could not load the contract interface for contract abstraction {abstractionName}");

            return contract;

        }

        /// <summary>
        /// Updates a abstraction definition from a source logic model.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="abstractionSource">The source abstraction interface to update.</param>
        /// <param name="logicContract">The logic contract used to update the abstraction.</param>
        /// <returns>The up to date version of the abstraction contract.</returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task<CsInterface> UpdateAbstractionContractAsync(this IVsActions source, VsCSharpSource abstractionSource, CsInterface logicContract)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the web abstraction contract.");
            if (abstractionSource == null) throw new CodeFactoryException("No contract source was provided, cannot update the web abstraction contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the web abstraction contract.");

            CsInterface result = abstractionSource.SourceCode.Interfaces.FirstOrDefault();

            if (result == null) throw new CodeFactoryException("Could note load the contract interface cannot update the web abstraction contract.");

            var implementedMembers = result.FormatCSharpComparisonMembers(CodeFactory.DotNet.MemberComparisonType.Security);

            var sourceMembers = logicContract.FormatCSharpComparisonMembers(CodeFactory.DotNet.MemberComparisonType.Security);

            var contractMethods = new List<CsMethod>();

            foreach (var logicMember in sourceMembers)
            {
                if (implementedMembers.Any(i => i.Key == logicMember.Key)) continue;

                if (logicMember.Value.ModelType == CsModelType.Method) contractMethods.Add(logicMember.Value as CsMethod);
            }

            if (!contractMethods.Any()) return result;

            var manualNamespaceManager = new ManualNamespaceManager(result.Namespace);
            manualNamespaceManager.AddExistingUsingStatements(abstractionSource.SourceCode.NamespaceReferences);

            var namespaceManager = manualNamespaceManager.BuildNamespaceManager();
            var content = source.BuildAbstractionContractContent(contractMethods,namespaceManager);

            if (string.IsNullOrEmpty(content)) return result;

            var updateSource = await result.AddToEndAsync(content);

            if (updateSource == null) throw new CodeFactoryException($"Could not load the updated content from the abstraction {result.Name}");
            result = updateSource.Interfaces.FirstOrDefault();

            if (result == null) throw new CodeFactoryException("Could not load the interface definition for the updated abstraction contract.");

            return result;

        }

        /// <summary>
        /// Builds the abstraction content from provided methods.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractMethods">Contract methods to be added to the abstraction content.</param>
        /// <returns>Formatted abstraction content content.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or logic errors occurred.</exception>
        public static string BuildAbstractionContractContent(this IVsActions source, List<CsMethod> contractMethods,NamespaceManager manager)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot define the web abstraction content.");
            if (contractMethods == null) throw new CodeFactoryException("No contract methods were provided cannot define the web abstraction content.");

            if (!contractMethods.Any()) return null;

            var contentFormatter = new SourceFormatter();

            foreach (var contractMethod in contractMethods)
            {

                if (contractMethod.HasDocumentation)
                {
                    var doc = contractMethod.CSharpFormatXmlDocumentationEnumerator();

                    foreach (var docLine in doc)
                    {
                        contentFormatter.AppendCodeLine(2, docLine);
                    }
                }

                if (contractMethod.HasAttributes)
                {
                    foreach (var attributeData in contractMethod.Attributes)
                    {
                        contentFormatter.AppendCodeLine(2, attributeData.CSharpFormatAttributeSignature(manager));
                    }
                }

                contentFormatter.AppendCodeLine(2, $"{contractMethod.CSharpFormatInterfaceMethodSignature(manager)};");
                contentFormatter.AppendCodeLine(2);
            }

            return contentFormatter.ReturnSource();
        }

        /// <summary>
        /// Checks to see that a service url configuration class exists for the target module. If it does not it will create the definition.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="abstractionProject">Project that holds the abstraction implementation.</param>
        /// <exception cref="CodeFactoryException">Raised if data is missing or logic errors occurred.</exception>
        public static async Task CheckAbstractionServiceUrlAsync(this IVsActions source,VsProject abstractionProject)
        {
            if (source == null) throw new CodeFactoryException("CodeFactory automation cannot be accessed, cannot check for the service url configuration class.");

            var serviceUrlName = $"RpcServiceUrl";

            var project = abstractionProject;

            if (project == null) throw new CodeFactoryException("Could not access the web abstraction project, cannot check for the service url configuration class.");

            var serviceUrlSource = await project.FindCSharpSourceByClassNameAsync(serviceUrlName);

            if (serviceUrlSource != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service;");
            sourceFormatter.AppendCodeLine(0, $"namespace {project.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");

            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Service url.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public class {serviceUrlName} : ServiceUrl");
            sourceFormatter.AppendCodeLine(1, "{");

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates an instance of the service url.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"url\">URL for accessing the service.</param>");
            sourceFormatter.AppendCodeLine(2, $"public {serviceUrlName}(string url) : base(url)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "//Intentionally blank.");
            sourceFormatter.AppendCodeLine(2, "}");

            sourceFormatter.AppendCodeLine(1, "}");

            sourceFormatter.AppendCodeLine(0, "}");

            var serviceUrlDoc = await project.AddDocumentAsync($"{serviceUrlName}.cs", sourceFormatter.ReturnSource());

            if (serviceUrlDoc == null) throw new CodeFactoryException($"Could not load the service url document for '{serviceUrlName}' cannot confirm the server url configuration class was created.");

        }
    }
}