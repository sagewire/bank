using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports.extensions
{
    public static class StringExtensions
    {
        private static Regex _conceptKey = new Regex(@"{(?<concept>\w+?)\.(?<action>\w+?)\.(?<property>\w+?)}", RegexOptions.Compiled);

        public static string ConceptReplace(this string text, List<Concept> concepts)
        {
            if (text == null || concepts == null || !concepts.Any())
            {
                return text;
            }

            if (_conceptKey.IsMatch(text))
            {
                var match = _conceptKey.Match(text);

                var concept = concepts.FirstOrDefault(x => x.Name == match.Groups["concept"].Value.ToUpper());

                if (concept != null)
                {
                    switch(match.Groups["property"].Value.ToLower())
                    {
                        case "label":
                            return text.Replace(match.Value, concept.Label);
                    }
                }
            }

            return text;
        }
    }
}
