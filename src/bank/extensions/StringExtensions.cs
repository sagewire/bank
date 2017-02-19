using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace bank.extensions
{
    public static class StringExtensions
    {
        private static Regex _cleanRegex = new Regex("[^0-9 \\w]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _removeExtraSpaces = new Regex("[ ]{2,}", RegexOptions.Compiled);
        private static Regex _parameters = new Regex(@"{(?<key>[\w]+?)[\.}]", RegexOptions.Compiled);

        public static string DeSlugfiy(this string source)
        {
            // Note: Using proper case right now
            return source.Trim().Replace("-", " ");
        }

        public static string ToTitleCase(this string text, bool force = false)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            if (force)
            {
                text = text.ToLower();
            }
            return myTI.ToTitleCase(text);
        }

        public static string CreateSlug(this string source, int? maxLength = null, bool breakOnWord = false)
        {
            if (source == null) return source;

            source = source.Replace(".", "");
            source = _cleanRegex.Replace(source, " ");
            source = _removeExtraSpaces.Replace(source, " ").Trim();
            source = source.Replace(" ", "-").Trim();

            if (maxLength.HasValue && source.Length > maxLength.Value)
            {
                if (breakOnWord)
                {
                    while (source.LastIndexOf("-") > maxLength.Value)
                    {
                        source = source.Substring(0, source.LastIndexOf("-"));
                    }
                }
                else
                {
                    source = source.Substring(0, maxLength.Value);
                }

            }

            return source.ToLower();
        }

        public static string ParameterReplace(this string text, Dictionary<string, object> parameters)
        {
            if (text == null || parameters == null || !parameters.Any())
            {
                return text;
            }

            var matches = _parameters.Matches(text);

            foreach(Match match in matches)
            {
                var key = match.Groups["key"].Value;
                var keepBraces = match.Value.Contains(".");
                
                if (parameters.ContainsKey(key))
                {
                    var p = parameters[key];
                    text = text.Replace(keepBraces ? key : match.Value, p.ToString());
                }
            }

            return text;
        }

        
        public static string StripHTML(this string htmlString)
        {
            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(htmlString, pattern, string.Empty);
        }

        public static string RemoveSpaces(this string text)
        {
            return _removeExtraSpaces.Replace(text, " ").Trim();
        }

        public static string Chop(this string content, string[] separator)
        {
            var strSplitArr = content.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return strSplitArr[0];
        }

        public static string RemoveSymbols(this string text)
        {
            var characters = text.ToCharArray();

            IEnumerable<char> safeCharacters = characters
                .Where(o => char.IsSymbol(o) == false)
                .ToList();

            var count = safeCharacters.Count();

            if (characters.Length != count)
                text = new string(safeCharacters.ToArray());

            return text;
        }
        public static string SafeSubstring(this string input, int length)
        {
            return input.SafeSubstring(0, length);
        }

        public static string SafeSubstring(this string input, int startIndex, int length)
        {
            if (input.Length >= (startIndex + length))
                return input.Substring(startIndex, length);

            return input.Length > startIndex ? input.Substring(startIndex) : string.Empty;
        }

    }
}
