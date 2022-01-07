using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;

namespace CommonDeliveryFramework.Net.Automation.Common
{
/// <summary>
    /// C# Extensions class that support model implementation.
    /// </summary>
    public static class ModelExtensions
    {

        /// <summary>
        /// Attribute name to be added to all gRpc model that convert from APP model.
        /// </summary>
        public const string ModelConvertAttribute = "ModelConvertAttribute";

        /// <summary>
        /// Namespace in which the model support items are located. 
        /// </summary>
        public const string ModelUtilityNamespace = "CommonDeliveryFramework";


        /// <summary>
        /// Locates a target model that implements the transformation for a source model.
        /// </summary>
        /// <param name="source">The project to search.</param>
        /// <param name="sourceModel">The source class model used to find the target model in the project.</param>
        /// <param name="targetModelName">The proposed name of the target model that implements transformation of source model.</param>
        /// <returns>The source code file the target model was found in.</returns>
        public static async Task<VsCSharpSource> FindModel(this VsProject source, CsClass sourceModel, string targetModelName)
        {
            //Bounds checking
            if (source == null) return null;

            if (string.IsNullOrEmpty(targetModelName)) return null;

            var children = await source.GetChildrenAsync(true, true);

            var sourceCode = children.Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>();

            var result = sourceCode.FirstOrDefault(s => s.SourceCode.Classes.Any(c => 
                                                            c.Attributes.Any( a => a.Type.Namespace == ModelUtilityNamespace & a.Type.Name == ModelConvertAttribute & a.Parameters.Any(p => p?.Value.TypeValue.Namespace == sourceModel.Namespace & p?.Value.TypeValue.Name == sourceModel.Name))) 
                                                        | s.SourceCode.Classes.Any(c=> c.Name == targetModelName));

            return result;
        }

        /// <summary>
        /// Checks a type definition to determine if it is a target class from a target namespace.
        /// </summary>
        /// <param name="source">The source type to validate.</param>
        /// <param name="rootNamespace">The name space root to check for.</param>
        /// <returns>True if it matches the criteria, false if not.</returns>
        public static bool IsModel(this CsType source, string rootNamespace)
        {
            if (source == null) return false;
            if (string.IsNullOrEmpty(rootNamespace)) return false;


            if (source.IsWellKnownType) return false;

            if (!source.IsClass) return false;

            return source.Namespace.StartsWith(rootNamespace);
        }

        /// <summary>
        /// Creates a model name space from the folder structure and source project.
        /// </summary>
        /// <param name="source">The project the name space will be hosted in.</param>
        /// <param name="folderStructure">The folder structure in parent to child order.</param>
        /// <returns>The formatted name space.</returns>
        public static string CreateModelNamespace(this VsProject source, IReadOnlyList<string> folderStructure)
        {
            if (source == null) return null;

            string result = null;
            if (folderStructure != null)
            {
                if (folderStructure.Any())
                {
                    StringBuilder namespaceBuilder = new StringBuilder();

                    namespaceBuilder.Append(source.DefaultNamespace);
                    foreach (var folder in folderStructure)
                    {
                        namespaceBuilder.Append($".{folder}");
                    }

                    result = namespaceBuilder.ToString();
                }
                else
                {
                    result = source.DefaultNamespace;
                }
            }
            else
            {
                result = source.DefaultNamespace;
            }

            return result;
        }

        /// <summary>
        /// Determines if the target source code file has a class with properties.
        /// </summary>
        /// <param name="source">Source code file to search.</param>
        /// <returns>True properties found, false if not.</returns>
        public static bool HasClassProperties(this VsCSharpSource source)
        {
            if(source == null) return false;

            var sourceCode = source.SourceCode;
            if (sourceCode == null) return false;

            return sourceCode.Classes.Any(c => c.Properties.Any());
        }

        /// <summary>
        /// Creates a standard property definition from the property, with the target type of the property being passed in.
        /// </summary>
        /// <param name="source">Property to generate.</param>
        /// <param name="targetType">The target type to be used with the property.</param>
        /// <returns>Formatted syntax for a property.</returns>
        /// <exception cref="CodeFactoryException">Raised if data is missing</exception>
        public static string FormatCSharpProperty(this CsProperty source, string targetType)
        {
            if (source == null) throw new CodeFactoryException("No property data was provided cannot format the c# property syntax.");

            if (string.IsNullOrEmpty(targetType)) throw new CodeFactoryException("No target type was provided cannot format the c# property syntax");

            StringBuilder propertySyntax = new StringBuilder();

            propertySyntax.Append($"{source.Security.FormatCSharpSyntax()} {targetType} {source.Name} {{ ");
            if (source.HasGet & source.GetSecurity == CsSecurity.Public) propertySyntax.Append("get; ");
            if (source.HasSet & source.SetSecurity == CsSecurity.Public) propertySyntax.Append("set; ");
            propertySyntax.Append("}");

            return propertySyntax.ToString();
        }

        /// <summary>
        /// Gets the c# type definition syntax from a type, if the type is nullable it will return the target type as no longer nullable.
        /// </summary>
        /// <param name="source">Source type to evaluate. </param>
        /// <returns>Target type syntax as not nullable.</returns>
        /// <exception cref="CodeFactoryException">Raised if no type data can be found.</exception>
        public static string GetPropertyTypeDefinitionSyntaxRemoveNullableFromDefinition(this CsType source)
        {
            var propertyType = source;

            if (propertyType == null) throw new CodeFactoryException($"Could not get the type information from the property '{source.Name}'.");

            string result = null;
            if (propertyType.Namespace == "System" & propertyType.Name == "Nullable")
            {
                var sourceType = propertyType.GenericTypes.FirstOrDefault();

                if (sourceType == null) throw new CodeFactoryException($"Could not get the nullable type information for property '{source.Name}'");

                result = sourceType.CSharpFormatTypeName();
            }
            else result = propertyType.CSharpFormatTypeName();

            return result;
        }

        /// <summary>
        /// Property extension that returns either the name of the property or the method to get the default value of the property if it is nullable.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static string DefaultValueCSharpSyntax(this CsProperty source)
        {

            if (source == null) throw new CodeFactoryException("The property was not provided cannot define the default value for this property.");
            string result = null;

            var propertyType = source.PropertyType;

            if (propertyType == null) throw new CodeFactoryException($"Could not get the property type for property '{source.Name}' cannot define the default value for this property.");

            if (propertyType.Namespace == "System" & propertyType.Name == "Nullable")
            {
                var sourceType = propertyType.GenericTypes.FirstOrDefault();

                if (sourceType == null) throw new CodeFactoryException($"Cannot get target type of the nullable type. cannot define the default vale for the property '{source.Name}'");

                if (sourceType.IsWellKnownType)
                {
                    switch (sourceType.WellKnownType)
                    {
                        case CsKnownLanguageType.Boolean:
                            result = $"{source.Name}.GetValueOrDefault(false)";
                            break;
                        case CsKnownLanguageType.Character:
                            result = $"{source.Name}.GetValueOrDefault(char.MinValue)";
                            break;
                        case CsKnownLanguageType.Signed8BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.UnSigned8BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Signed16BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Unsigned16BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Signed32BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Unsigned32BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Signed64BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Unsigned64BitInteger:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Decimal:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Single:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.Double:
                            result = $"{source.Name}.GetValueOrDefault(0)";
                            break;
                        case CsKnownLanguageType.DateTime:
                            result = $"{source.Name}.GetValueOrDefault(DateTime.MinValue)";
                            break;
                        default:
                            result = $"{source.Name}.GetValueOrDefault()";
                            break;
                    }
                }
                else
                {
                    if (sourceType.Namespace == "System" & sourceType.Name == "Guid") result = $"{source.Name}.GetValueOrDefault(Guid.Empty)";
                    else result = $"{source.Name}.GetValueOrDefault()";
                }
            }
            else result = source.Name;

            return result;
        }

    }
}