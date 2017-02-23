using System;
using System.Linq;

namespace PA.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert string to string array using first character as separator
        /// </summary>
        /// <returns>The array.</returns>
        /// <param name="value">Value.</param>
        public static string[] AsArray(this string value)
        {
            return value.Split(new char[] { value[0] }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Convert string to TargetType array using first character as separator
        /// </summary>
        /// <returns>The array.</returns>
        /// <param name="value">string value.</param>
        /// <param name="TargetType">Target type.</param>
        /// <param name="getInstance">Get instance.</param>
        public static Array AsArray(this string value, Type TargetType, Func<Type, string, object> getInstance)
        {
            if (TargetType.HasElementType)
            {
                Type type = TargetType.GetElementType();

                if (type.HasElementType)
                {
                    return value.AsArray().AsEnumerable().Select(a => a.AsArray(type, getInstance)).ToArray(type);
                }
                else
                {
                    return value.AsArray().AsEnumerable().Select(s => getInstance(type, s)).ToArray(type);
                }
            }

            throw new InvalidCastException("Not an array");
        }

        /// <summary>
        /// Convert string to pascal case
        /// </summary>
        /// <returns>The pascal cased string</returns>
        /// <param name="str">String.</param>
        public static string ToPascalCase(this string str)
        {
            // If there are 0 or 1 characters, just return the string.
            if (str == null)
                return str;
            if (str.Length < 2)
                return str.ToUpper();

            // Split the string into words.
            string[] words = str.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result += word.Substring(0, 1).ToUpper() + word.Substring(1);
            }
            return result;
        }

        /// <summary>
        /// Convert string to camel case
        /// </summary>
        /// <returns>The camel case.</returns>
        /// <param name="str">String.</param>
        public static string ToCamelCase(this string str)
        {
            // If there are 0 or 1 characters, just return the string.
            if (str == null || str.Length < 2)
                return str;

            // Split the string into words.
            string[] words = str.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result += words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
            }

            return result;
        }
    }
}
