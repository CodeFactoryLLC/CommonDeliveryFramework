using System.Linq;

namespace CommonDeliveryFramework.Net.Automation.Common
{
    /// <summary>
    /// Extension methods that handle string formatting.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Formats a string as camel case.
        /// </summary>
        /// <param name="source">The source string to format as camel case.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatCamelCase(this string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var first = source.First().ToString().ToLowerInvariant();
            return source.Length > 1 ?$"{first}{source.Substring(1)}":first;
        }

        /// <summary>
        /// Formats a string as proper case.
        /// </summary>
        /// <param name="source">the source string to format as proper case.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatProperCase(this string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var first = source.First().ToString().ToUpperInvariant();
            return source.Length > 1 ? $"{first}{source.Substring(1)}" : first;
        }
    }
}