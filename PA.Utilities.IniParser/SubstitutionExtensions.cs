using IniParser.Model;
using System;
using System.Text.RegularExpressions;

namespace PA.Utilities.IniParser
{
    public static class SubstitutionExtensions
    {
        /// <summary>
        ///     Attempts to retrieve a key, using a single string combining section and 
        ///     key name. 
        /// 	Substitute ${refkey} with corresponding values, refkey is formalized as parameter <paramref name="key"/> 	
        /// </summary>
        /// <param name="key">
        ///     The section and key name to retrieve, separated by <see cref="IniData.SectionKeySeparator"/>.
        /// 
        ///     If key contains no separator, it is treated as a key in the <see cref="IniData.Global"/> section.
        /// 
        ///     Key may contain no more than one separator character.
        /// </param>
        /// <param name="value">
        ///     If true is returned, is set to the value retrieved.  Otherwise, is set
        ///     to an empty string.
        /// </param>
        /// <returns>
        ///     True if key was found, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     key contained multiple separators.
        /// </exception>
        public static void TryGetSubstituedKey(this IniData inidata, string key, out string value)
        {
            inidata.TryGetKey(key, out value);
            value = inidata.Substitute(value);
        }

        /// <summary>
        ///     Retrieves a key using a single input string combining section and key name.
        /// 	Substitute ${refkey} with corresponding values, refkey is formalized as parameter <paramref name="key"/> 	
        /// </summary>
        /// <param name="key">
        ///     The section and key name to retrieve, separated by <see cref="IniData.SectionKeySeparator"/>.
        /// 
        ///     If key contains no separator, it is treated as a key in the <see cref="IniData.Global"/> section.
        /// 
        ///     Key may contain no more than one separator character.
        /// </param>
        /// <returns>
        ///     The key's value if it was found, otherwise null.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     key contained multiple separators.
        /// </exception>
        public static string GetSubstituedKey(this IniData inidata, string key)
        {
            return inidata.Substitute(inidata.GetKey(key));
        }

        /// <summary>
        /// 	Substitute ${refkey} in <paramref name="value"/> with corresponding values, refkey is formalized as the
        /// 	section and key name to retrieve, separated by <see cref="IniData.SectionKeySeparator"/>.
        /// 
        /// 	If refkey contains no separator, it is treated as a key in the <see cref="IniData.Global"/> section.
        /// 
        /// 	refkey may contain no more than one separator character.
        /// </summary>
        /// <returns>The substituted string</returns>
        /// <param name="value">String with variables to substitute</param>
        /// <exception cref="ArgumentException">
        ///     key contained multiple separators.
        /// </exception>
        public static string Substitute(this IniData inidata, string value)
        {
            return Regex.Replace(value, @"\$\{([^}]+)\}", m => inidata.GetSubstituedKey(m.Groups[1].Value));
        }

    }

}