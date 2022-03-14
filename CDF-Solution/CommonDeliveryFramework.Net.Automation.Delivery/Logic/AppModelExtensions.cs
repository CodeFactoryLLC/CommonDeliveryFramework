using CodeFactory.DotNet.CSharp;

namespace CommonDeliveryFramework.Net.Automation.Delivery.Logic
{
    /// <summary>
    /// Extensions class that handles the creation and management of application models.
    /// </summary>
    public static class AppModelExtensions
    {
        /// <summary>
        /// Checks a type to see if it is an application model definition.
        /// </summary>
        /// <param name="source">Source type to check.</param>
        /// <returns>True if it is an app model or false if not.</returns>
        public static bool IsAppModel(this CsType source)
        { 
            if(source == null) return false;

            return source.Namespace.Contains(DeliveryInfo.AppModelLibrarySuffix);
        }
    }
}