using System.Linq;
using CodeFactory;
using CodeFactory.DotNet.CSharp;

namespace CommonDeliveryFramework.Net.Automation.Common
{
    /// <summary>
    /// Extensions and supporting logic for <see cref="CsInterface"/>
    /// </summary>
    public static class CsInterfaceExtensions
    {
        /// <summary>
        /// Converts a interface into a standard class name format.
        /// </summary>
        /// <param name="source">Interface model to extract the class name from.</param>
        /// <returns></returns>
        /// <exception cref="CodeFactoryException">Data is missing or a processing error occurred.</exception>
        public static string GetClassName(this CsInterface source)
        {
            if (source == null) throw new CodeFactoryException("The interface model was not provided, cannot get a class name from the interface.");

            var interfaceName = source.Name;

            if (string.IsNullOrEmpty(interfaceName)) throw new CodeFactoryException("No interface name could be found cannot get a class name from the interface.");

            string result = null;
            if (interfaceName.FirstOrDefault().ToString().ToUpper() == "I")
            {
                result = interfaceName.Length > 1 ? interfaceName.Substring(1): interfaceName;
            }
            else
            {
                result = interfaceName;
            }

            return result.FormatProperCase();
        }
    }
}