using System;
using System.Linq;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;
using CommonDeliveryFramework.Net.Automation.Common;

namespace CommonDeliveryFramework.Net.Automation.Delivery.Logic
{
    public static class DependencyInjectionHelpers
    {
        public static async Task RegisterTransientServicesAsync(this VsProject source)
        {
            
            if(source == null) return;
            
            var loaders = await source.GetProjectLoadersAsync();

            if(!loaders.Any()) return;

            var transientClasses =  await source.LoadInstanceProjectClassesForTransientRegistrationAsync();

            foreach (var loaderSource in loaders)
            {
                var loaderClasses = loaderSource.Classes.Where(c =>
                    c.BaseClass.Name == "DependencyInjectionLibraryLoader" &
                    c.BaseClass.Namespace == "CommonDeliveryFramework");

                foreach (var loaderClass in loaderClasses)
                {
                    var manager = loaderSource.LoadNamespaceManager(source.DefaultNamespace);

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
    }
}