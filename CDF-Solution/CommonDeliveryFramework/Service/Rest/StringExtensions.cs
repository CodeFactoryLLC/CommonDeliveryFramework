﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDeliveryFramework.Service.Rest
{
    /// <summary>
    /// Data class that manages extensions for the <see cref="string"/> type.
    /// </summary>
    public static  class StringExtensions
    {
        /// <summary>
        /// Place holder value used when passing strings in rest.
        /// </summary>
        private static string RestPostPlaceHolderValueForString = "~~~Empty~~~";

        /// <summary>
        /// Extension method that sets the post value for a string. If the string is null or empty will send a formatted string to represent empty or null.
        /// </summary>
        /// <param name="source">Source string to set.</param>
        /// <returns>The formatted string value.</returns>
        public static string SetPostValue(this string source)
        {
            return string.IsNullOrEmpty(source) ? RestPostPlaceHolderValueForString : source;
        }

        /// <summary>
        /// Extension method that gets the received value from a post. Will check for the empty value to convert the result to null or will pass the returned response.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetPostValue(this string source)
        {
            return source != RestPostPlaceHolderValueForString ? source : null;
        }
    }
}
