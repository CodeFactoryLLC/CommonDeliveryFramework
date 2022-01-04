using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;


namespace CommonDeliveryFramework.Net.Automation.Common
{

    /// <summary>
    /// Extensions class that provides automation support for dependency injection. 
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        //Logger used for code factory logging
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger _logger = LogManager.GetLogger("DependencyInjectionExtensions");

        /// <summary>
        /// Helper method that confirms a target project supports the microsoft extensions for dependency injection and Configuration.
        /// </summary>
        /// <param name="sourceProject">Target project to check.</param>
        /// <returns>True if found or false of not.</returns>
        public static async Task<bool> HasMicrosoftExtensionDependencyInjectionLibrariesAsync(this VsProject sourceProject)
        {
            if (sourceProject == null) return false;
            if (!sourceProject.IsLoaded) return false;
            var references = await sourceProject.GetProjectReferencesAsync();

            //Checking for dependency injection libraries.
            bool returnResult = references.Any(r => r.Name == "Microsoft.Extensions.DependencyInjection.Abstractions");
            if (!returnResult) return false;

            //Checking for the configuration libraries.
            returnResult = references.Any(r => r.Name == "Microsoft.Extensions.Configuration.Abstractions");
            return returnResult;
        }

        /// <summary>
        /// Gets all source code files from the root level of the project that implement CommonDeliveryFramework.DependencyInjectionLibraryLoader base class.
        /// </summary>
        /// <param name="sourceProject">The project to search.</param>
        /// <returns>Source code files where dependency injection loader library is implemented.</returns>
        public static async Task<IReadOnlyList<CsSource>> GetProjectLoadersAsync(this VsProject sourceProject)
        {
            ImmutableList<CsSource> results = ImmutableList<CsSource>.Empty;

            //Bounds checking
            if (sourceProject == null) return results;
            if (!sourceProject.IsLoaded) return results;
            if (!sourceProject.HasChildren) return results;

            //Getting the root level project files and folders from the project.
            var projectChildren = await sourceProject.GetChildrenAsync(false, true);

            //Filtering out everything that is not C# source code file and grabbing all source code files that have a class that implement CommonDeliveryFramework.DependencyInjectionLibraryLoader base class.
            var loaders = projectChildren.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>()
                .Where(s => s.SourceCode.Classes.Any(c => c.BaseClass.Name == "DependencyInjectionLibraryLoader" & c.BaseClass.Namespace == "CommonDeliveryFramework" ))
                .Select(c => c.SourceCode);

            if (loaders.Any()) results = results.AddRange(loaders);

            return results;
        }

        /// <summary>
        /// Loads all the classes that exist in the project from each code file found within the project. That qualify for transient dependency injection.
        /// </summary>
        /// <param name="project">The source project to get the classes from</param>
        /// <returns>The class models for all classes that qualify for transient dependency injection. If no classes are found an empty enumeration will be returned.</returns>
        public static async Task<IEnumerable<CsClass>> LoadInstanceProjectClassesForTransientRegistrationAsync(this VsProject project)
        {
            var result = new List<CsClass>();
            if (project == null) return result;
            if (!project.HasChildren) return result;

            try
            {
                var projectChildren = await project.GetChildrenAsync(true, true);

                var csSourceCodeDocuments = projectChildren
                    .Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                    .Cast<VsCSharpSource>();

                foreach (var csSourceCodeDocument in csSourceCodeDocuments)
                {
                    var sourceCode = csSourceCodeDocument.SourceCode;
                    if (sourceCode == null) continue;
                    if (!sourceCode.Classes.Any()) continue;
                    var classes = sourceCode.Classes.Where(IsTransientClass).Where(c =>
                        result.All(r => $"{c.Namespace}.{c.Name}" != $"{r.Namespace}.{r.Name}"));

                    if (classes.Any()) result.AddRange(classes);
                }

            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while loading the classes to be added to dependency injection.",
                    unhandledError);
            }

            return result;
        }

        /// <summary>
        /// Checks class data to determine if it qualifies for transient dependency injection.
        /// - Checks to make sure the class only has 1 interface defined.
        /// - Checks to see the class only has 1 constructor defined.
        /// - Checks to see if the class is a asp.net controller if it it remove it
        /// - Checks to see the class name is a startup class if so will be removed.
        /// - Confirms the constructor has no well known types if so will be removed.
        /// </summary>
        /// <param name="classData">The class data to check.</param>
        /// <returns>Boolean state if it qualifies.</returns>
        public static bool IsTransientClass(CsClass classData)
        {
            if (classData == null) return false;

            if (classData.IsStatic) return false;
            if (classData.InheritedInterfaces.Any()) if (classData.InheritedInterfaces.Count > 1) return false;
            if (!classData.Constructors.Any()) return false;
            if (classData.IsController()) return false;
            if (classData.IsRazorComponent()) return false;
            if (classData.IsGrpcService()) return false;
            if (classData.Constructors.Count > 1) return false;

            var constructor = classData.Constructors.FirstOrDefault(m => m.HasParameters);

            if (constructor == null) return false;

            if (classData.Name == "Startup") return false;

            return !constructor.Parameters.Any(p => p.ParameterType.IsWellKnownType);
        }

        /// <summary>
        /// Builds the services registration method. This will contain the transient registrations for each class in the target project.
        /// This will return a signature of [Public/Private] [static] void [methodName](IServiceCollection [collectionParameterName])
        /// With a body that contains the full transient registrations.
        /// </summary>
        /// <param name="classes">The classes to be added.</param>
        /// <param name="manager">The namespace manager that will be used to shorten type name registration with dependency injection. This will need to be loaded from the target class.</param>
        /// <returns>The formatted method.</returns>
        public static string BuildInjectionMethod(IEnumerable<CsClass> classes, NamespaceManager manager = null)
        {

            CodeFactory.SourceFormatter registrationFormatter = new CodeFactory.SourceFormatter();

            registrationFormatter.AppendCodeLine(0, "/// <inheritdoc />");
            registrationFormatter.AppendCodeLine(0, "protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)");
            registrationFormatter.AppendCodeLine(0, "{");
            registrationFormatter.AppendCodeLine(1, "//This method was auto generated, do not modify by hand!");
            foreach (var csClass in classes)
            {
                var registration = csClass.FormatTransientRegistration(manager);
                if (registration != null) registrationFormatter.AppendCodeLine(1, registration);
            }
            registrationFormatter.AppendCodeLine(0, "}");

            return registrationFormatter.ReturnSource();
        }

        /// <summary>
        /// Defines the transient registration statement that will register the class.
        /// </summary>
        /// <param name="classData">The class model to get the registration from.</param>
        /// <param name="manager">Optional parameter that contains the namespace manager that contains the known using statements and target namespace for the class that will host this registration data.</param>
        /// <returns>The formatted transient registration call or null if the class does not meet the criteria.</returns>
        private static string FormatTransientRegistration(this CsClass classData,  NamespaceManager manager = null)
        {
            //Cannot find the class data will return null
            if (classData == null) return null;

            string registrationType = null;
            string classType = null;

            ICsMethod constructorData = classData.Constructors.FirstOrDefault();

            //Confirming we have a constructor 
            if (constructorData == null) return null;

            //Getting the fully qualified type name for the formatters library for the class.
            classType = classData.CSharpFormatBaseTypeName(manager);

            //if we are not able to format the class name correctly return null.
            if (classType == null) return null;

            //Assuming the first interface inherited will be used for dependency injection if any are provided.
            if (classData.InheritedInterfaces.Any())
            {
                CsInterface interfaceData = classData.InheritedInterfaces.FirstOrDefault();

                if (interfaceData != null) registrationType = interfaceData.CSharpFormatInheritanceTypeName(manager);
            }

            //Creating statement to add the the container.
            string diStatement = registrationType != null
                ? $"serviceCollection.AddTransient<{registrationType},{classType}>();" :
                  $"serviceCollection.AddTransient<{classType}>();";

            return diStatement;
        }
    }
}
