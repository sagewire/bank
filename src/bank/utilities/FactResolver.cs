using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.poco;
using NCalc;

namespace bank.utilities
{
    public class FactResolver
    {
        private static Regex _nameList = new Regex(@"((?<name>\w{8})\|*)+", RegexOptions.Compiled);
        //private static Regex _concepts = new Regex(@"\$(?<concept>[\w\d]{8})(?=\W)", RegexOptions.Compiled);

        //public IList<string> ParseConcepts(string text)
        //{
        //    var results = new List<string>();
        //    foreach (Match match in _concepts.Matches(" " + text + " "))
        //    {
        //        results.Add(match.Groups["concept"].Value);
        //    }

        //    return results;
        //}

        public object Evaluate(string text, IDictionary<string, Fact> facts)
        {

            if (text.Length > 8)
            {
                if (_nameList.IsMatch(text))
                {
                    return EvaluateList(text, facts);
                }
                else
                {
                    return EvaluateFormula(text, facts);
                }
            }
            else if (facts.ContainsKey(text))
            {
                var fact = facts[text];
                return fact.NumericValue;
            }
            else
            {
                return null;
                //throw new Exception(string.Format("Fact not found {0}", text));
            }
        }

        public object EvaluateList(string text, IDictionary<string, Fact> facts)
        {
            foreach (Capture name in _nameList.Match(text).Groups["name"].Captures)
            {
                if (facts.ContainsKey(name.Value))
                {
                    var fact = facts[name.Value];
                    return fact.NumericValue.Value;
                }
            }

            return null;
        }

        public object EvaluateFormula(string text, IDictionary<string, Fact> facts)
        {
            var candidate = text;
            var discardResult = false;

            var e = new Expression(candidate);
            e.EvaluateParameter += (name, args) =>
            {
                if (facts.ContainsKey(name))
                {
                    args.Result = facts[name].NumericValue;
                }
                else
                {
                    discardResult = true;
                    args.Result = 1;
                }

            };

            object result = null;
            try
            {
                result = e.Evaluate();

                if (discardResult)
                {
                    result = null;
                }
            }
            catch
            {
                Console.WriteLine();
            }

            return result;
        }

        //public string Replace(string text, IDictionary<string, Fact> facts)
        //{
        //    return _concepts.Replace(" " + text + " ", m =>
        //    {
        //        var concept = m.Groups["concept"].Value;

        //        if (facts.ContainsKey(concept))
        //        {
        //            return facts[concept].NumericValue.ToString();
        //        }
        //        else
        //        {
        //            return concept;
        //        }

        //    }).Trim();
        //}

    }
}
