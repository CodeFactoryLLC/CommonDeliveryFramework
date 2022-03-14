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
    /// Extensions class that provides logic to implement services and abstractions that support Rest.
    /// </summary>
    public static class ServiceRestExtensions
    {
        /// <summary>
        /// Updates a Rest service implementation to be in sync with the supporting logic implementation.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="restModelProject">The project that hosts the rest models.</param>
        /// <param name="restProject">The project that implements the rest functionality.</param>
        /// <param name="appModelProject">The project that hosts the application models.</param>
        /// <returns>Updated class definition of the implemented service.</returns>
        public static async Task<CsClass> UpdateRestServiceAsync(this IVsActions source, CsInterface logicContract,
            VsProject restModelProject, VsProject restProject, VsProject appModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot update the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot update the service.");

            var modelProject = restModelProject;

            if (modelProject == null)
                throw new CodeFactoryException("Could not load the rest model project cannot find the request type.");


            VsProject serviceProject = restProject;

            if (serviceProject == null) throw new CodeFactoryException("Could not load the rest service project, cannot update the service.");

            if (appModelProject == null) throw new CodeFactoryException("Could not load the application model project cannot update the service.");

            var children = await serviceProject.GetChildrenAsync(false, false);

            var serviceFolder = children.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>().FirstOrDefault(f => f.Name == "Controllers");

            if (serviceFolder == null) throw new CodeFactoryException("Could not access the 'Controllers' folder from the rest (WebApi) project cannot update the service.");

            var serviceName = logicContract.GetClassName();

            var folderChildren = await serviceFolder.GetChildrenAsync(true, true);

            var serviceSource = await serviceProject.FindCSharpSourceByClassNameAsync($"{serviceName}Controller");

            return serviceSource == null ? await source.NewRestServiceAsync(logicContract,restProject,appModelProject,restModelProject): await source.UpdateRestServiceImplementationAsync(serviceSource,logicContract,restModelProject);
        }

        /// <summary>
        /// Create a new Rest service implementation to be in sync with the supporting logic implementation.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="restProject">The project that holds the rest implementation.</param>
        /// <param name="appModelProject">The project that holds the application model definitions.</param>
        /// <param name="restModelProject">The project that contains the rest models.</param>
        /// <returns>Created class definition of the implemented service.</returns>
        public static async Task<CsClass> NewRestServiceAsync(this IVsActions source, CsInterface logicContract,
            VsProject restProject,VsProject appModelProject, VsProject restModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service.");

            VsProject serviceProject = restProject;

            if (serviceProject == null) throw new CodeFactoryException("Could not load the rest service project, cannot update the service.");

            var children = await serviceProject.GetChildrenAsync(false, false);

            var serviceFolder = children.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder).Cast<VsProjectFolder>().FirstOrDefault(f => f.Name == "Controllers");

            if (serviceFolder == null) throw new CodeFactoryException("Could not access the 'Controllers' folder from the rest project cannot update the service.");

            var sourceNamespace = await serviceFolder.GetCSharpNamespaceAsync();
            if (string.IsNullOrEmpty(sourceNamespace)) throw new CodeFactoryException("Could not load the target namespace for the rest service.");

            var serviceName = logicContract.GetClassName();

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
            manualNamespaceManager.AddUsingStatement("Microsoft.AspNetCore.Mvc");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.AspNetCore.Mvc;");
            manualNamespaceManager.AddUsingStatement(appModelProject.DefaultNamespace);
            sourceFormatter.AppendCodeLine(0, $"using {appModelProject.DefaultNamespace};");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework");
            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework;");
            manualNamespaceManager.AddUsingStatement("CommonDeliveryFramework.Service.Rest");
            sourceFormatter.AppendCodeLine(0, "using CommonDeliveryFramework.Service.Rest;");
            manualNamespaceManager.AddUsingStatement(restModelProject.DefaultNamespace);
            sourceFormatter.AppendCodeLine(0, $"using {restModelProject.DefaultNamespace};");

            var manager = manualNamespaceManager.BuildNamespaceManager();


            sourceFormatter.AppendCodeLine(0, $"namespace {sourceNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Rest service implementation of the logic contract <see cref=\"{logicContract.Name}\"/>");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, "[Route(\"api/[controller]\")]");
            sourceFormatter.AppendCodeLine(1, "[ApiController]");
            sourceFormatter.AppendCodeLine(1, $"public  class {serviceName}Controller:ControllerBase");
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
            sourceFormatter.AppendCodeLine(2, $"/// Creates a instance of the controller/>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger that supports this controller.</param>");
            sourceFormatter.AppendCodeLine(2, $"/// <param name=\"{logicParameter}\">Logic contract implemented by this controller.</param>");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"public {serviceName}Controller(ILogger<{serviceName}Controller> logger,{logicContract.Name} {logicParameter})");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger = logger;");
            sourceFormatter.AppendCodeLine(3, $"{logicVariable} = {logicParameter};");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);

            List<CsMethod> serviceMethods = logicContract.Methods.ToList();

            if (serviceMethods.Any()) sourceFormatter.AppendCode(await source.BuildRestServiceLogicAsync(serviceMethods, logicContract,restModelProject,manager));

            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            var sourceDocument = await serviceFolder.AddDocumentAsync($"{serviceName}Controller.cs",sourceFormatter.ReturnSource());

            if (sourceDocument == null) throw new CodeFactoryException($"Was not able to load the created service implementation for '{serviceName}Controller'");

            var serviceSource = await sourceDocument.GetCSharpSourceModelAsync();

            if (serviceSource == null) throw new CodeFactoryException($"Was not able to load the created service source code implementation for '{serviceName}Controller'");


            return serviceSource.Classes.FirstOrDefault();
        }

        /// <summary>
        /// Updates a Rest service implementation to be in sync with the supporting service contract.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="serviceSource">Source code for the existing rest service implementation.</param>
        /// <param name="logicContract">The logic contract interface definition.</param>
        /// <param name="restModelProject">The rest model project that all rest data is stored.</param>
        /// <returns>Updated class definition of the implemented service.</returns>
        public static async Task<CsClass> UpdateRestServiceImplementationAsync(this IVsActions source,
            VsCSharpSource serviceSource, CsInterface logicContract,VsProject restModelProject)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service.");
            if (serviceSource == null) throw new CodeFactoryException("No source was found for the rest service, cannot update the rest service.");
            if (restModelProject == null) throw new CodeFactoryException("No rest model project was provided cannot create the service.");

            var currentSource = serviceSource.SourceCode;

            var serviceClass = currentSource.Classes.FirstOrDefault();

            if (serviceClass == null) throw new CodeFactoryException("Could not load the service class definition from the source cannot update the rest service.");

            var serviceMethods = serviceClass.Methods.Where(m => m.MethodType != CsMethodType.Constructor).ToList();

            foreach (var serviceMethod in serviceMethods)
            {
                var currentMethod = serviceClass?.GetModel(serviceMethod.LookupPath) as CsMethod;

                if(currentMethod == null) continue;

                currentSource = await currentMethod.DeleteAsync();

                serviceClass = currentSource.Classes.FirstOrDefault();

                if (serviceClass == null) throw new CodeFactoryException("Could not load the service class definition from the source cannot update the rest service.");
            }

            var manager = new NamespaceManager(currentSource.NamespaceReferences, serviceClass?.Namespace);

            var logicMethods = logicContract.Methods.ToList();

            if (!logicMethods.Any()) return serviceClass;

            var sourceCode = await source.BuildRestServiceLogicAsync(logicMethods, logicContract,restModelProject, manager);

            if (serviceClass == null) throw new CodeFactoryException("Could not load the service class definition from the source cannot update the rest service.");

            if (string.IsNullOrEmpty(sourceCode)) return serviceClass;

            currentSource = await serviceClass.AddToEndAsync(sourceCode);

            return currentSource.Classes.FirstOrDefault();
        }

        /// <summary>
        /// Builds rest service logic implementation for a target service and logic contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="serviceMethods">The service methods and the target logic method that supports the service implementation.</param>
        /// <param name="logicContract">The logic contract that is being supported by the service contract.</param>
        /// <param name="restModelProject">The model project for rest models.</param>
        /// <param name="manager">Namespace manager for formatting content.</param>
        /// <returns>Formatted service logic.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is not provided, or a logic error occurs.</exception>
        public static async Task<string> BuildRestServiceLogicAsync(this IVsActions source,
            List<CsMethod> serviceMethods, CsInterface logicContract, VsProject restModelProject, NamespaceManager manager)
        {
            if (source == null) throw new CodeFactoryException("Cannot access CodeFactory automation cannot create the service logic.");
            if (serviceMethods == null) throw new CodeFactoryException("No service methods were provided cannot create service logic.");
            if (logicContract == null) throw new CodeFactoryException("No logic contract was provided cannot create the service logic.");

            var logicVariable = $"_{logicContract.GetClassName().FormatCamelCase()}";

            var serviceName = logicContract.GetClassName();

            SourceFormatter logicFormatter = new SourceFormatter();

            var logicMethods = logicContract.Methods;


            foreach (var methodData in serviceMethods)
            {
                var logicMethod = methodData;

                if (logicMethod == null) throw new CodeFactoryException("Could not load the logic method data, cannot create service logic.");

                bool isOverload = logicMethods.Count(m => m.Name == logicMethod.Name) > 1;

                string serviceCallName = logicMethod.GetRestName(isOverload);

                string serviceMethodName = $"{serviceCallName}Async";

                bool isPost = logicMethod.IsPostCall();

                string returnTypeSyntax = null;

                bool usesRequestModel = logicMethod.Parameters.Count > 1;

                CsClass requestModel = usesRequestModel ? await source.GetRestRequestAsync(logicMethod, serviceName,restModelProject) : null;

                string parameterName = usesRequestModel ? "request" : logicMethod.HasParameters ? logicMethod.Parameters[0].Name : null;
                string parameterType = usesRequestModel ? requestModel.Name :  logicMethod.HasParameters ? logicMethod.Parameters[0].ParameterType.CSharpFormatTypeName(manager) : null;

                bool isNullBoundsCheck = false;
                bool isStringBoundsCheck = false;
                bool isLogicAsyncMethod = logicMethod.ReturnType.IsTaskType();
                CsType logicReturnType = logicMethod.ReturnType.TaskReturnType();

                bool returnsData = logicReturnType != null;

                returnTypeSyntax = logicReturnType == null ? "NoDataResult" : $"ServiceResult<{logicReturnType.CSharpFormatTypeName(manager)}>";


                logicFormatter.AppendCodeLine(2,"/// <summary>");
                logicFormatter.AppendCodeLine(2,$"/// Service implementation for the logic method '{logicMethod.Name}'");
                logicFormatter.AppendCodeLine(2,"/// </summary>");
                if (!isPost)
                {
                    logicFormatter.AppendCodeLine(2,$"[HttpGet(\"{serviceCallName}\")]");
                    logicFormatter.AppendCodeLine(2, $"public async Task<ActionResult<{returnTypeSyntax}>> {serviceMethodName}()");
                }
                else
                {
                    logicFormatter.AppendCodeLine(2,$"[HttpPost(\"{serviceCallName}\")]");
                    logicFormatter.AppendCodeLine(2, $"public async Task<ActionResult<{returnTypeSyntax}>> {serviceMethodName}([FromBody]{parameterType} {parameterName})");
                }

                logicFormatter.AppendCodeLine(2,"{");

                logicFormatter.AppendCodeLine(3,"_logger.InformationEnterLog();");
                logicFormatter.AppendCodeLine(3);

                if (logicMethod.HasParameters)
                {
                    if (usesRequestModel) isNullBoundsCheck = true;
                    else
                    {
                        if (logicMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String) isStringBoundsCheck = true;
                        else if (!logicMethod.Parameters[0].ParameterType.IsValueType) isNullBoundsCheck = true;
                    }

                    if (isNullBoundsCheck)
                    {
                        logicFormatter.AppendCodeLine(3, $"if ({parameterName} == null)");
                        logicFormatter.AppendCodeLine(3, "{");
                        logicFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({parameterName})}} was not provided. Will raise an argument exception\");");
                        logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                        logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new ValidationException(nameof({parameterName})));");
                        logicFormatter.AppendCodeLine(3, "}");
                        logicFormatter.AppendCodeLine(3);
                    }

                    if (isStringBoundsCheck)
                    {
                        logicFormatter.AppendCodeLine(3, $"if(string.IsNullOrEmpty({parameterName}))");
                        logicFormatter.AppendCodeLine(3, "{");
                        logicFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({parameterName})}} was not provided. Will raise an argument exception\");");
                        logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                        logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new ValidationException(nameof({parameterName})));");
                        logicFormatter.AppendCodeLine(3, "}");
                        logicFormatter.AppendCodeLine(3);
                    }
                }

                if (logicReturnType != null)
                {
                    logicFormatter.AppendCodeLine(3, logicReturnType.IsValueType
                            ? $"{logicReturnType.CSharpFormatTypeName(manager)} result;"
                            : $"{logicReturnType.CSharpFormatTypeName(manager)} result = null;");
                }

                logicFormatter.AppendCodeLine(3,"try");
                logicFormatter.AppendCodeLine(3,"{");


                string returnValue = returnsData ? "result = " : "";
                string awaitStatement = isLogicAsyncMethod ? "await " : "";

                string formattedParameters = "";

                if (logicMethod.HasParameters)
                {
                    if (logicMethod.Parameters.Count == 1)
                    {
                        formattedParameters = logicMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String ? $"{parameterName}.GetPostValue()" : parameterName;
                    }
                    else
                    {
                        bool isFirstParameter = true;
                        StringBuilder logicStringBuilder = new StringBuilder();
                        foreach (var logicMethodParameter in logicMethod.Parameters)
                        {

                            if (isFirstParameter)
                            {
                                logicStringBuilder.Append(logicMethodParameter.ParameterType.WellKnownType == CsKnownLanguageType.String ? $"request.{logicMethodParameter.Name.FormatProperCase()}.GetPostValue()" : $"request.{logicMethodParameter.Name.FormatProperCase()}");
                                isFirstParameter = false;
                            }
                            else
                            {
                                logicStringBuilder.Append(logicMethodParameter.ParameterType.WellKnownType == CsKnownLanguageType.String ? $", request.{logicMethodParameter.Name.FormatProperCase()}.GetPostValue()" : $", request.{logicMethodParameter.Name.FormatProperCase()}");
                            }
                        }

                        formattedParameters = logicStringBuilder.ToString();
                    }
                }

                logicFormatter.AppendCodeLine(4,$"{returnValue}{awaitStatement} {logicVariable}.{logicMethod.Name}({formattedParameters});");
                

                logicFormatter.AppendCodeLine(3,"}");

                logicFormatter.AppendCodeLine(3, "catch (ManagedException managed)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"Raising the handled exception to the caller of the service.\");");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(managed);");
                logicFormatter.AppendCodeLine(3,"}");

                logicFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.CriticalLog(\"An unhandled exception occurred, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new UnhandledException());");
                logicFormatter.AppendCodeLine(3, "}");
                logicFormatter.AppendCodeLine(3);

                logicFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(3, returnsData ? $"return {returnTypeSyntax}.CreateResult(result);" : $"return {returnTypeSyntax}.CreateSuccess();");

                logicFormatter.AppendCodeLine(2,"}");
                logicFormatter.AppendCodeLine(2);
            }

            return logicFormatter.ReturnSource();
        }



        /// <summary>
        /// Generates the rest action name based on the method name and the supporting parameters of the method.
        /// </summary>
        /// <param name="source">The source method to extract the name from.</param>
        /// <param name="fullSignatureName">Flag that determines if the parameters of the method should also be used in the name rest call.</param>
        /// <returns>The formatted name of the rest call.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static string GetRestName(this CsMethod source,bool fullSignatureName = false)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot update the rest service, a method was not provided so cannot determine the name of the rest call.");

            if (string.IsNullOrEmpty(source.Name))
                throw new CodeFactoryException(
                    "Cannot update the rest service, the method name was empty or null, cannot determine the name of the rest call.");


            StringBuilder restNameBuilder = new StringBuilder();

            var methodName = source.Name.Trim();

            restNameBuilder.Append(methodName.EndsWith("Async", StringComparison.InvariantCultureIgnoreCase)
                ? methodName.Substring(0, methodName.Length - 5)
                : methodName);

            if (!(fullSignatureName & source.HasParameters)) return restNameBuilder.ToString();

            restNameBuilder.Append("By");

            foreach (var sourceParameter in source.Parameters)
            {
                if(string.IsNullOrEmpty(sourceParameter.Name)) continue;

                restNameBuilder.Append(sourceParameter.Name.FormatProperCase());
            }

            return restNameBuilder.ToString();
        }

        /// <summary>
        /// Determines of the source method will be called as a post call or not. 
        /// </summary>
        /// <param name="source">Target method to check for a post call.</param>
        /// <returns>True if the call will be post based or false if not.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static bool IsPostCall(this CsMethod source)
        {
            if (source == null)
                throw new CodeFactoryException("No method was provided cannot determine if a Post call.");
            
            return source.HasParameters;
        }

        /// <summary>
        /// Gets the request type based on the target method provided.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="sourceMethod">The method to get the request for.</param>
        /// <param name="serviceName">The name of the service the request will be supporting.</param>
        /// <param name="restModelProject">The rest model project to get and add the request.</param>
        /// <returns>The class definition of the request.</returns>
        /// <exception cref="CodeFactoryException">If required data is missing, or information cannot be loaded.</exception>
        public static async Task<CsClass> GetRestRequestAsync(this IVsActions source, CsMethod sourceMethod, string serviceName, VsProject restModelProject)
        {
            if (source == null) throw new CodeFactoryException("No CodeFactory automation was provided cannot find the request type.");
            if (sourceMethod == null) throw new CodeFactoryException("Cannot get the request type since no method was provided.");
            if (sourceMethod.Parameters.Count < 2)
                throw new CodeFactoryException(
                    "The provided method has one or less parameters does not qualify to generate a rest request type.");
            if (string.IsNullOrEmpty(serviceName))
                throw new CodeFactoryException("The service name was not provided cannot find the request type.");

            if (restModelProject == null)
                throw new CodeFactoryException("Could not load the rest model project cannot find the request type.");
            var requestName = sourceMethod.GetRestServiceRequestModelName(serviceName);

            var modelSource = await restModelProject.FindCSharpSourceByClassNameAsync(requestName);

            CsClass result = null;

            if (modelSource != null)
            {
                result = modelSource.SourceCode.Classes.FirstOrDefault();
            }
            else
            {
                SourceFormatter requestFormatter = new SourceFormatter();

                requestFormatter.AppendCodeLine(0,$"namespace {restModelProject.DefaultNamespace}");
                requestFormatter.AppendCodeLine(0,"{");
                requestFormatter.AppendCodeLine(0);
                requestFormatter.AppendCodeLine(1,$"public class {requestName}");
                requestFormatter.AppendCodeLine(1,"{");

                foreach (var sourceMethodParameter in sourceMethod.Parameters)
                {
                    requestFormatter.AppendCodeLine(2, $"public {sourceMethodParameter.ParameterType.FormatCSharpFullTypeName()} {sourceMethodParameter.Name.FormatProperCase()} {{ get; set;}}");
                    requestFormatter.AppendCodeLine(2);
                }

                requestFormatter.AppendCodeLine(1,"}");
                requestFormatter.AppendCodeLine(1);
                requestFormatter.AppendCodeLine(0,"}");

                var document = await restModelProject.AddDocumentAsync($"{requestName}.cs", requestFormatter.ReturnSource());

                var loadedSource = await document.GetCSharpSourceModelAsync();

                result = loadedSource.Classes.FirstOrDefault();
            }

            if (result == null)
                throw new CodeFactoryException($"Could not load the request model data for '{requestName}'.");

            return result;
        }

        /// <summary>
        /// Get the target name of a service request class. 
        /// </summary>
        /// <param name="source">The source method used to generate the request.</param>
        /// <param name="serviceName">The target name of the service being implemented.</param>
        /// <returns>The name of the request model or null if no request model is needed for this method.</returns>
        public static string GetRestServiceRequestModelName(this CsMethod source, string serviceName)
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
    }
}