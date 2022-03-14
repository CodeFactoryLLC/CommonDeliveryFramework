using System.Threading.Tasks;
using CodeFactory.VisualStudio;

namespace CommonDeliveryFramework.Net.Automation.Delivery.Logic
{
    /// <summary>
    /// Extension methods that support actions related to projects or from project models.
    /// </summary>
    public static class VsProjectExtensions
    {
        /// <summary>
        /// Extension method that confirm the source code file is part of the APP model project.
        /// </summary>
        /// <param name="source">The source code file to validate.</param>
        /// <returns>True if in the APP model project otherwise false.</returns>
        public static async Task<bool> IsAppModelProjectAsync(this VsCSharpSource source)
        {
            var hostProject = await source.GetHostingProjectAsync();

            if (hostProject == null) return false;
            return hostProject.Name.EndsWith(DeliveryInfo.AppModelLibrarySuffix);
        }


        /// <summary>
        /// Extension method that confirm the source code file is part of the Sql model project.
        /// </summary>
        /// <param name="source">The source code file to validate.</param>
        /// <returns>True if in the Sql model project otherwise false.</returns>
        public static async Task<bool> IsSqlModelProjectAsync(this VsCSharpSource source)
        {
            var hostProject = await source.GetHostingProjectAsync();

            if (hostProject == null) return false;
            return hostProject.Name.EndsWith(DeliveryInfo.SqlModelLibrarySuffix);
        }

        /// <summary>
        /// Extension method that confirm the source code file is part of the service rpc model project.
        /// </summary>
        /// <param name="source">The source code file to validate.</param>
        /// <returns>True if in the service rpc model project otherwise false.</returns>
        public static async Task<bool> IsServiceRpcModelProjectAsync(this VsCSharpSource source)
        {
            var hostProject = await source.GetHostingProjectAsync();

            if (hostProject == null) return false;
            return hostProject.Name.EndsWith(DeliveryInfo.GrpcModelLibrarySuffix);
        }

        /// <summary>
        /// Extension method that confirms the source code file is part of one of the logic projects.
        /// </summary>
        /// <param name="source">The source code file to validate.</param>
        /// <returns>True if in the service rpc model project otherwise false.</returns>
        public static async Task<bool> IsLogicProjectAsync(this VsCSharpSource source)
        {
            var hostProject = await source.GetHostingProjectAsync();

            if (hostProject == null) return false;
            return hostProject.Name.EndsWith(DeliveryInfo.LogicLibrarySuffix);
        } 
        
        
    }
}