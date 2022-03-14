using System.Linq;
using CodeFactory;
using CodeFactory.DotNet.CSharp;

namespace CommonDeliveryFramework.Net.Automation.Common
{
    /// <summary>
    /// Extension methods that support the <see cref="CsType"/>
    /// </summary>
    public static class CsTypeExtensions
    {
        /// <summary>
        /// Determines if the type is a task type.
        /// </summary>
        /// <param name="source">the source type to validate.</param>
        /// <returns>True if the type is a standard task or a generic task implementation. False otherwise.</returns>
        /// <exception cref="CodeFactoryException">If no type definition is provided.</exception>
        public static bool IsTaskType(this CsType source)
        {
            if (source == null) throw new CodeFactoryException("No type data was provided cannot determine if the type is a task type.");

            return (source.Namespace == "System.Threading.Tasks" & source.Name == "Task");
        }

        /// <summary>
        /// Returns the type definition for the target type that is supported by a task type. If the type is not a task type it will return the type definition.
        /// </summary>
        /// <param name="source">source type to check.</param>
        /// <returns>The target type or null if the type is void or a non generic task type.</returns>
        /// <exception cref="CodeFactoryException">If no type definition is provided or the generic task type has no type definition.</exception>
        public static CsType TaskReturnType(this CsType source)
        {
            if (source == null) throw new CodeFactoryException("No method data was provided cannot determine the return type.");

            if (!source.IsTaskType())
            {
                return source.WellKnownType == CsKnownLanguageType.Void ? null : source;
            }

            if (!source.IsGeneric) return null;

            var returnType = source.GenericTypes.FirstOrDefault();

            if(returnType == null) throw new CodeFactoryException("Could not load the type definition from the generic task type."); 
            
            return returnType;
        }
    }
}