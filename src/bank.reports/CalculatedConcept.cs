using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace bank.reports
{
    public class CalculatedConcept : Concept
    {
        private static Regex _concepts = new Regex(@"\$(?<concept>[\w\d]{8})(?=\W)", RegexOptions.Compiled);

        public static bool IsCalculatedConcept(string value)
        {
            return _concepts.IsMatch(value);
        }

        public CalculatedConcept(string value) : base(null)
        {
            ConceptKeys = new List<string>();

            MatchCollection matches = _concepts.Matches(value);

            foreach(Match match in matches)
            {
                ConceptKeys.Add(match.Groups["concept"].Value);
            }
        }
    }
}
