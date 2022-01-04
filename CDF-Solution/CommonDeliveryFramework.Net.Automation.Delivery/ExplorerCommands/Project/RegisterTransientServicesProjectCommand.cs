using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Delivery.ExplorerCommands.Project
{
/// <summary>
    /// Code factory command for automation of a project when selected from solution explorer.
    /// </summary>
    public class RegisterTransientServicesProjectCommand : ProjectCommandBase
    {
        private static readonly string commandTitle = "RegisterTransientServices";
        private static readonly string commandDescription = "Rebuilds the registration of transient service registration for dependency injection.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RegisterTransientServicesProjectCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }
#pragma warning disable CS1998
        #region Overrides of VsCommandBase<VsProject>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsProject result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                isEnabled = await result.HasMicrosoftExtensionDependencyInjectionLibrariesAsync();
                IReadOnlyList<CsSource> loaders = null;
                if (isEnabled) loaders = await result.GetProjectLoadersAsync();

                if (loaders != null) isEnabled = loaders.Any();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while checking if the solution explorer project command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsProject result)
        {
            try
            {
                var loaders = await result.GetProjectLoadersAsync();

                if(!loaders.Any()) return;

                var transientClasses =  await result.LoadInstanceProjectClassesForTransientRegistrationAsync();

                foreach (var loaderSource in loaders)
                {
                    var loaderClasses = loaderSource.Classes.Where(c =>
                        c.BaseClass.Name == "DependencyInjectionLibraryLoader" &
                        c.BaseClass.Namespace == "CommonDeliveryFramework");

                    foreach (var loaderClass in loaderClasses)
                    {
                        var manager = loaderSource.LoadNamespaceManager(result.DefaultNamespace);

                        var loadRegistrationMethod =
                            DependencyInjectionExtensions.BuildInjectionMethod(transientClasses, manager);

                        if(string.IsNullOrEmpty(loadRegistrationMethod)) continue;

                        var registrationMethod = new SourceFormatter();

                        registrationMethod.AppendCodeBlock(2, loadRegistrationMethod);

                        //If the registration method is not being replaced but added new adding an additional indent level. 
                        string newRegistrationMethod = registrationMethod.ReturnSource();

                        var currentRegistrationMethod =
                            loaderClass.Methods.FirstOrDefault(m => m.Name == "LoadRegistration");

                        
                        if (currentRegistrationMethod != null)
                            await currentRegistrationMethod.ReplaceAsync(loadRegistrationMethod);
                        else await loaderClass.AddToEndAsync(newRegistrationMethod);

                    }
                }

                
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer project command {commandTitle}. ",
                    unhandledError);

            }
        }

        #endregion
    }
}
