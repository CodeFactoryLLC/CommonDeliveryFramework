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

namespace CommonDeliveryFramework.Net.Automation.Service.Rest
{
    /// <summary>
    /// Extensions class that supports web abstractions and supporting logic.
    /// </summary>
    public static class RestWebAbstractionExtensions
    {

        /// <summary>
        /// Updates a rest abstraction implementation, by building upon a service and logic implementation.
        /// </summary>
        /// <param name="source">CodeFactory Automation</param>
        /// <param name="logicContract">The logic contract to support for the abstraction.</param>
        /// <param name="serviceClass">The service class implementation to call.</param>
        /// <param name="appModelProject">Project that hosts application models.</param>
        /// <param name="restModelProject">Project that hosts rest models.</param>
        /// <param name="abstractionProject">Project that hosts the implementation of the abstraction.</param>
        /// <param name="abstractionSuffix">The suffix for an abstraction implementation.</param>
        public static async Task UpdateWebRestAbstraction(this IVsActions source, CsInterface logicContract,
            CsClass serviceClass,VsProject appModelProject, VsProject restModelProject, VsProject abstractionProject, string abstractionSuffix)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot access CodeFactory automation cannot update the web abstraction.");
            if (logicContract == null)
                throw new CodeFactoryException(
                    "No logic contract was provided cannot create cannot update the web abstraction.");
            if (serviceClass == null)
                throw new CodeFactoryException("No service contract was provided  cannot update the web abstraction.");


            var abstractProject = abstractionProject;
            if (abstractProject == null)
                throw new CodeFactoryException(
                    "Could not load the abstraction project cannot update the abstraction.");


            var abstractionContractSource =
                await abstractionProject.FindCSharpSourceByInterfaceNameAsync($"IRest{logicContract.GetClassName()}{abstractionSuffix}");

            var abstractionContract = abstractionContractSource != null
                ? await source.UpdateRestAbstractionContractAsync(abstractionContractSource, logicContract)
                : await source.NewRestAbstractionContractAsync(logicContract,appModelProject,restModelProject,abstractionProject,abstractionSuffix);

            if (abstractionContract == null)
                throw new CodeFactoryException(
                    "Could note load the abstraction contract interface cannot update the web abstraction.");

            var abstractClassName = abstractionContract.GetClassName();

            var abstractClassSource = await abstractionProject.FindCSharpSourceByClassNameAsync(abstractClassName);

            var abstraction = abstractClassSource == null
                ? await source.NewRestAbstractionAsync(abstractionContract, serviceClass,appModelProject,restModelProject,abstractionProject)
                : await source.UpdateRestAbstractionAsync(abstractClassSource, abstractionContract, serviceClass);

            if (abstraction == null)
                throw new CodeFactoryException(
                    $"Could not load the updated abstraction '{abstractClassName}' cannot confirm if the update was completed.");
        }

        /// <summary>
        /// Creates a new web abstraction from scratch.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="abstractContract">The abstraction contract to implement.</param>
        /// <param name="serviceClass">The service class to be used with the abstractions.</param>
        /// <param name="appModelProject">Project that hosts application models.</param>
        /// <param name="restModelProject">Project that hosts rest models.</param>
        /// <param name="abstractionProject">Project that hosts the implementation of the abstraction.</param>
        /// <returns>The implemented abstraction class.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or a logic error occurred.</exception>
        public static async Task<CsClass> NewRestAbstractionAsync(this IVsActions source, CsInterface abstractContract,
            CsClass serviceClass, VsProject appModelProject, VsProject restModelProject, VsProject abstractionProject)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot access CodeFactory automation cannot update the web abstraction.");
            if (abstractContract == null)
                throw new CodeFactoryException(
                    "No logic contract was provided cannot create cannot update the web abstraction.");
            if (serviceClass == null)
                throw new CodeFactoryException("No service contract was provided  cannot update the web abstraction.");

            var abstractProject = abstractionProject;
            if (abstractProject == null)
                throw new CodeFactoryException(
                    "Could not load the web abstraction project cannot create the web abstraction contract.");


            var abstractionClassName = abstractContract.GetClassName();

            var abstractionNamespace = abstractionProject.DefaultNamespace;
            if (string.IsNullOrEmpty(abstractionNamespace))
                throw new CodeFactoryException("Could not create the namespace for the web abstraction contract.");

            await source.CheckRestAbstractionServiceUrlAsync(abstractProject);

            var serviceUrlParameter = $"restServiceUrl";

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
            abstractionFormatter.AppendCodeLine(0, "using System.Net.Http.Json;");
            manualNamespaceManager.AddUsingStatement("System.Net.Http.Json");
            abstractionFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            manualNamespaceManager.AddUsingStatement("Microsoft.Extensions.Logging");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            abstractionFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Service");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Service.Rest");
            abstractionFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service.Rest;");
            manualNamespaceManager.AddUsingStatement(restModelProject.DefaultNamespace);
            abstractionFormatter.AppendCodeLine(0, $"using {restModelProject.DefaultNamespace};");

            abstractionFormatter.AppendCodeLine(0, $"namespace {abstractionNamespace}");
            abstractionFormatter.AppendCodeLine(0, "{");

            abstractionFormatter.AppendCodeLine(1, "/// <summary>");
            abstractionFormatter.AppendCodeLine(1,
                $"/// Web abstraction implementation for abstraction contract '{abstractContract.Name}'");
            abstractionFormatter.AppendCodeLine(1, "/// </summary>");
            abstractionFormatter.AppendCodeLine(1, $"public class {abstractionClassName}:{abstractContract.Name}");
            abstractionFormatter.AppendCodeLine(1, "{");
            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Url for the service being accessed by this abstraction.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, $"private readonly ServiceUrl _{serviceUrlParameter};");
            abstractionFormatter.AppendCodeLine(2);

            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Logger used for the class.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            abstractionFormatter.AppendCodeLine(2);

            abstractionFormatter.AppendCodeLine(2, "/// <summary>");
            abstractionFormatter.AppendCodeLine(2, "/// Creates a instance of the web abstraction.");
            abstractionFormatter.AppendCodeLine(2, "/// </summary>");
            abstractionFormatter.AppendCodeLine(2,
                "/// <param name=\"logger\">Logger that supports this abstraction.</param>");
            abstractionFormatter.AppendCodeLine(2, "/// <param name=\"url\">service url</param>");
            abstractionFormatter.AppendCodeLine(2,
                $"public {abstractionClassName}(ILogger<{abstractionClassName}> logger, RestServiceUrl url)");
            abstractionFormatter.AppendCodeLine(2, "{");

            abstractionFormatter.AppendCodeLine(3, "_logger = logger;");
            abstractionFormatter.AppendCodeLine(3, $"_{serviceUrlParameter} = url;");

            abstractionFormatter.AppendCodeLine(2, "}");
            abstractionFormatter.AppendCodeLine(2);

            var contractMethods = abstractContract.Methods.ToList();

            if (contractMethods.Any())
            {
                var contentMethods = new List<CsMethod>();
                foreach (var contractMethod in contractMethods)
                {
                    var isOverload = contractMethods.Count(m => m.Name == contractMethod.Name) > 1;
                    var serviceMethodName = $"{contractMethod.GetRestName(isOverload)}Async";

                    var serviceMethod = serviceClass.Methods.FirstOrDefault(m => m.Name == serviceMethodName);

                    if (serviceMethod != null) contentMethods.Add(contractMethod);
                }

                if (contentMethods.Any())
                {
                    var namespaceManager = manualNamespaceManager.BuildNamespaceManager();
                    var content = source.BuildRestAbstractionContent(contentMethods, serviceClass, namespaceManager);

                    if (!string.IsNullOrEmpty(content)) abstractionFormatter.AppendCode(content);
                }
            }

            abstractionFormatter.AppendCodeLine(1, "}");
            abstractionFormatter.AppendCodeLine(0, "}");

            var abstractionDocument =
                await abstractProject.AddDocumentAsync($"{abstractionClassName}.cs", abstractionFormatter.ReturnSource());
            if (abstractionDocument == null)
                throw new CodeFactoryException($"Could not load the document for abstraction {abstractionClassName}");

            var abstractionSource = await abstractionDocument.GetCSharpSourceModelAsync();
            if (abstractionSource == null)
                throw new CodeFactoryException(
                    $"Could not load the source code for abstraction {abstractionClassName}");

            CsClass abstraction = abstractionSource.Classes.FirstOrDefault();

            if (abstraction == null)
                throw new CodeFactoryException(
                    $"Could not load the contract class for abstraction {abstractionClassName}");

            return abstraction;

        }

        /// <summary>
        /// Updates an existing web abstraction.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="abstractSource">The source code for the abstraction to update.</param>
        /// <param name="abstractContract">The abstraction contract to make sure is implemented.</param>
        /// <param name="serviceClass">The service contract that is called by the abstraction.</param>
        /// <returns>class definition that has been implemented.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or a logic error occurred.</exception>
        public static async Task<CsClass> UpdateRestAbstractionAsync(this IVsActions source,
            VsCSharpSource abstractSource, CsInterface abstractContract, CsClass serviceClass)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot access CodeFactory automation cannot update the web abstraction.");
            if (abstractSource == null)
                throw new CodeFactoryException("No abstraction source was provided cannot update the web abstraction.");
            if (abstractContract == null)
                throw new CodeFactoryException("No logic contract was provided cannot update the web abstraction.");
            if (serviceClass == null)
                throw new CodeFactoryException("No service contract was provided cannot update the web abstraction.");


            var abstractClass = abstractSource.SourceCode.Classes.FirstOrDefault();

            if (abstractClass == null)
                throw new CodeFactoryException(
                    $"Cannot load the web abstraction class cannot update the web abstraction.");

            var contractMethods = abstractContract.Methods.ToList();

            var abstractionMethods =
                abstractClass.Methods.Where(m => m.MethodType != CsMethodType.Constructor).ToList();

            var currentSource = abstractSource.SourceCode;

            foreach (var abstractionMethod in abstractionMethods)
            {
                var removeMethod = abstractClass.GetModel(abstractionMethod.LookupPath) as CsMethod;

                if(removeMethod == null) continue;

                currentSource = await removeMethod.DeleteAsync();

                abstractClass = currentSource.Classes.FirstOrDefault();

                if (abstractClass == null) throw new CodeFactoryException("Could not load the existing abstraction class cannot update the abstraction.");

            }

            if (contractMethods.Any())
            {
                var contentMethods = new List<CsMethod>();
                foreach (var contractMethod in contractMethods)
                {
                    var isOverload = contractMethods.Count(m => m.Name == contractMethod.Name) > 1;
                    var serviceMethodName = $"{contractMethod.GetRestName(isOverload)}Async";

                    var serviceMethod = serviceClass.Methods.FirstOrDefault(m => m.Name == serviceMethodName);

                    if (serviceMethod != null) contentMethods.Add(contractMethod);
                    
                }

                if (contentMethods.Any())
                {
                    var namespaceManager = new NamespaceManager(abstractSource.SourceCode.NamespaceReferences,
                        abstractClass.Namespace);
                    var content = source.BuildRestAbstractionContent(contentMethods, serviceClass, namespaceManager);

                    var updatedSource = await abstractClass.AddToEndAsync(content);

                    if (updatedSource == null)
                        throw new CodeFactoryException(
                            "Could not load the updated source code for the abstraction class.");

                    abstractClass = updatedSource.Classes.FirstOrDefault();

                    if (abstractClass == null)
                        throw new CodeFactoryException(
                            "Could not load the updated source code class for the abstraction");

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
        /// <param name="serviceClass">The service class implementation that is supported by this abstraction.</param>
        /// <param name="manager">The namespace manager used for type formatting.</param>
        /// <returns>Formatted methods.</returns>
        /// <exception cref="CodeFactoryException">If data is missing or a logic error occurred.</exception>
        public static string BuildRestAbstractionContent(this IVsActions source, List<CsMethod> abstractionMethods,
            CsClass serviceClass, NamespaceManager manager)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot access CodeFactory automation cannot create content for a web abstraction.");
            if (abstractionMethods == null)
                throw new CodeFactoryException(
                    "No abstraction methods to implement were provided, cannot create content for a web abstraction.");
            if (serviceClass == null)
                throw new CodeFactoryException(
                    "No service class was provided, cannot create content for a web abstraction");
            if (!abstractionMethods.Any())
                throw new CodeFactoryException(
                    "No abstraction methods to implement were provided, cannot create content for a web abstraction.");

            var contentFormatter = new SourceFormatter();

            string serviceName = serviceClass.Name.Replace("Controller", "");

            var serviceUrlParameter = $"_restServiceUrl";

            foreach (var contractMethod in abstractionMethods)
            {

                bool isOverLoad = abstractionMethods.Count(m => m.Name == contractMethod.Name) > 1;
                bool isPost = contractMethod.IsPostCall();

                string actionName = contractMethod.GetRestName(isOverLoad);
                

                CsType logicReturnType = contractMethod.ReturnType.TaskReturnType();

                bool returnsData = logicReturnType != null;

                string returnTypeSyntax = logicReturnType == null
                    ? "NoDataResult"
                    : $"ServiceResult<{logicReturnType.CSharpFormatTypeName(manager)}>";

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

                contentFormatter.AppendCodeLine(2,
                    $"{contractMethod.CSharpFormatStandardMethodSignatureWithAsync(manager)}");
                contentFormatter.AppendCodeLine(2, "{");

                contentFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");


                if (contractMethod.HasParameters)
                {

                    foreach (ICsParameter paramData in contractMethod.Parameters)
                    {
                        //If the parameter has a default value then continue will not bounds check parameters with a default value.
                        if (paramData.HasDefaultValue) continue;

                        //If the parameter is a string type add the following bounds check
                        if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String &
                            !paramData.HasDefaultValue)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if(string.IsNullOrEmpty({paramData.Name}))");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4,
                                $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }

                        // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                        if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType &
                            !paramData.HasDefaultValue)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if({paramData.Name} == null)");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4,
                                $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }
                    }
                }

                if (returnsData)
                {
                    contentFormatter.AppendCodeLine(3,
                        logicReturnType.IsClass
                            ? $"{logicReturnType.CSharpFormatTypeName(manager)} result = null; "
                            : $"{logicReturnType.CSharpFormatTypeName(manager)} result;");
                    contentFormatter.AppendCodeLine(3);
                }

                contentFormatter.AppendCodeLine(3, "try");
                contentFormatter.AppendCodeLine(3, "{");

                contentFormatter.AppendCodeLine(4,
                    $"using (HttpClient httpClient = new HttpClient(new HttpClientHandler {{ UseDefaultCredentials = true }}))");
                contentFormatter.AppendCodeLine(4, "{");

                if (isPost)
                {
                    if (contractMethod.Parameters.Count == 1)
                    {
                        var parameter = contractMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String
                                ? $"{contractMethod.Parameters[0].Name}.SetPostValue()"
                                : contractMethod.Parameters[0].Name;

                        contentFormatter.AppendCodeLine(5, $"var serviceData = await httpClient.PostAsJsonAsync<{contractMethod.Parameters[0].ParameterType.CSharpFormatTypeName(manager)}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\", {parameter});");
                        contentFormatter.AppendCodeLine(5);
                    }
                    else
                    {
                        var requestBuilder = new StringBuilder();
                        bool isFirst = true;

                        foreach (var parameter in contractMethod.Parameters)
                        {
                            var formattedParameter = parameter.ParameterType.WellKnownType == CsKnownLanguageType.String
                                ? $"{parameter.Name}.SetPostValue()"
                                : parameter.Name;

                            if (isFirst)
                            {

                                requestBuilder.Append($"{parameter.Name.FormatProperCase()} = {formattedParameter}");
                                isFirst = false;
                            }
                            else
                            {
                                requestBuilder.Append($", {parameter.Name.FormatProperCase()} = {formattedParameter}");
                            }
                        }

                        var requestName = contractMethod.GetRestServiceRequestModelName(serviceName);
                        contentFormatter.AppendCodeLine(5, $"var serviceData = await httpClient.PostAsJsonAsync<{requestName}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\", new {requestName} {{ {requestBuilder} }});");
                        contentFormatter.AppendCodeLine(5);
                    }

                    contentFormatter.AppendCodeLine(5, "serviceData.EnsureSuccessStatusCode();");
                    contentFormatter.AppendCodeLine(5);
                    contentFormatter.AppendCodeLine(5, $"var serviceResult = await serviceData.Content.ReadFromJsonAsync<{returnTypeSyntax}>();");
                    contentFormatter.AppendCodeLine(5);

                }
                else
                {
                    contentFormatter.AppendCodeLine(5, $"var serviceResult = await httpClient.GetFromJsonAsync<{returnTypeSyntax}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\");");
                    contentFormatter.AppendCodeLine(5);
                }

                contentFormatter.AppendCodeLine(5, "if (serviceResult == null) throw new ManagedException(\"Internal error occurred no data was returned\");");
                contentFormatter.AppendCodeLine(5);
                contentFormatter.AppendCodeLine(5, "serviceResult.RaiseException();");
                contentFormatter.AppendCodeLine(5);
                    
                if (returnsData)
                {
                    contentFormatter.AppendCodeLine(5, "result = serviceResult.Result;");
                    contentFormatter.AppendCodeLine(5);
                }

                contentFormatter.AppendCodeLine(4, "}");

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

                if (returnsData) contentFormatter.AppendCodeLine(3, "return result;");

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
        /// <param name="appModelProject">Project that hosts application models.</param>
        /// <param name="restModelProject">Project that hosts rest models.</param>
        /// <param name="abstractionProject">Project that hosts the implementation of the abstraction.</param>
        /// <param name="abstractionSuffix">The suffix for an abstraction implementation.</param>
        /// <returns>The implemented interface.</returns>
        public static async Task<CsInterface> NewRestAbstractionContractAsync(this IVsActions source, CsInterface logicContract, VsProject appModelProject, VsProject restModelProject, VsProject abstractionProject, string abstractionSuffix)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the web abstraction contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the web abstraction contract.");

            var abstractProject = abstractionProject;
            if (abstractProject == null) throw new CodeFactoryException("Could not load the web abstraction project cannot create the web abstraction contract.");

            var abstractionName = $"IRest{logicContract.GetClassName()}{abstractionSuffix}";

            var abstractionNamespace = abstractionProject.DefaultNamespace;
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
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            contractFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Service.Rest");
            contractFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service.Rest;");
            manualNamespaceManager.AddUsingStatement(restModelProject.DefaultNamespace);
            contractFormatter.AppendCodeLine(0, $"using {restModelProject.DefaultNamespace};");

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
                var content = source.BuildRestAbstractionContractContent(contractMethods,namespaceManager);

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
        public static async Task<CsInterface> UpdateRestAbstractionContractAsync(this IVsActions source, VsCSharpSource abstractionSource, CsInterface logicContract)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the web abstraction contract.");
            if (abstractionSource == null) throw new CodeFactoryException("No contract source was provided, cannot update the web abstraction contract.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the web abstraction contract.");

            CsInterface result = null;

            

            var contractMethods = logicContract.Methods.ToList();

            var currentSource = abstractionSource.SourceCode;

            var currentInterface = currentSource.Interfaces.FirstOrDefault();
            
            if (currentInterface == null) throw new CodeFactoryException("Could note load the contract interface cannot update the web abstraction contract.");

            var currentMethods = currentInterface.Methods.ToList();

            foreach (var currentMethod in currentMethods)
            {

                var deleteMethod = currentInterface.GetModel(currentMethod.LookupPath) as CsMethod;
                
                if(deleteMethod == null) continue;

                currentSource = await deleteMethod.DeleteAsync();

                currentInterface = currentSource.Interfaces.FirstOrDefault();

                if (currentInterface == null) throw new CodeFactoryException("Could note load the contract interface cannot update the web abstraction contract.");
            }

            if (!contractMethods.Any()) return currentInterface;

            var manualNamespaceManager = new ManualNamespaceManager(currentInterface.Namespace);
            manualNamespaceManager.AddExistingUsingStatements(abstractionSource.SourceCode.NamespaceReferences);

            var namespaceManager = manualNamespaceManager.BuildNamespaceManager();
            var content = source.BuildRestAbstractionContractContent(contractMethods,namespaceManager);

            if (string.IsNullOrEmpty(content)) return result;

            var updateSource = await currentInterface.AddToEndAsync(content);

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
        /// <param name="manager">The namespace manager used to validate the type formatting is implemented.</param>
        /// <returns>Formatted abstraction content content.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing or logic errors occurred.</exception>
        public static string BuildRestAbstractionContractContent(this IVsActions source, List<CsMethod> contractMethods,NamespaceManager manager)
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
        /// <param name="abstractionProject">The abstraction project that hosts the service url.</param>
        /// <exception cref="CodeFactoryException">Raised if data is missing or logic errors occurred.</exception>
        public static async Task CheckRestAbstractionServiceUrlAsync(this IVsActions source, VsProject abstractionProject)
        {
            if (source == null) throw new CodeFactoryException("CodeFactory automation cannot be accessed, cannot check for the service url configuration class.");


            var serviceUrlName = "RestServiceUrl";

            var project = abstractionProject;

            if (project == null) throw new CodeFactoryException("Could not access the web abstraction project, cannot check for the service url configuration class.");

            var serviceUrlSource = await project.FindCSharpSourceByClassNameAsync(serviceUrlName);

            if (serviceUrlSource != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service;");
            sourceFormatter.AppendCodeLine(0, $"namespace {project.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");

            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Service url that holds the uri to call services.");
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