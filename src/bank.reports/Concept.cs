using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.poco;
using bank.reports.formulas;
using bank.utilities;

namespace bank.reports
{
    [DebuggerDisplay("{Value}")]
    public class Concept
    {
        private static Regex _concepts = new Regex(@"[\w~_][\w\d]{7}", RegexOptions.Compiled);

        public Concept(string value)
        {
            Value = value;
            MatchCollection matches = _concepts.Matches(value);

            if (matches.Count > 0)
            {
                ConceptKeys = new List<string>();

                foreach (Match match in matches)
                {


                    if (Global.Concepts.ContainsKey(match.Value))
                    {
                        var globalConcept = Global.Concepts[match.Value];
                        Label = globalConcept.Label;
                        Name = globalConcept.Name;
                        Narrative = globalConcept.Narrative;
                        ShortLabel = globalConcept.ShortLabel;
                        Value = globalConcept.Value;

                        ConceptKeys.AddRange(globalConcept.ConceptKeys);
                    }
                    else
                    {
                        ConceptKeys.Add(match.Value);
                    }
                }
            }
        }

        public string Name { get; set; }
        public string ShortLabel { get; set; }
        public string Label { get; set; }
        public string Narrative { get; set; }
        public string Value { get; set; }
        public List<string> ConceptKeys { get; set; }

        public Fact PrepareFact(Fact fact)
        {
            return PrepareFact(new Fact[] { fact });
        }

        public Fact PrepareFact(IList<Fact> facts)
        {
            var resolver = new FactResolver();

            var result = resolver.Evaluate(Value, facts.ToDictionary(x => x.Name, x => x));

            var preparedFact = new Fact();
            preparedFact.NumericValue = (decimal)result;
            preparedFact.Period = facts.First().Period;

            var periods = new List<DateTime>();

            foreach (var fact in facts)
            {
                foreach (var item in fact.HistoricalData)
                {
                    periods.Add(item.Key);
                }
            }


            foreach (var period in periods.Distinct().OrderBy(x => x))
            {
                var filtered = facts.Select(x => new Fact
                {
                    Name = x.Name,
                    NumericValue = x.HistoricalData.ContainsKey(period) ?
                                          x.HistoricalData.Where(h => h.Key == period).First().Value :
                                          new decimal?()
                }).ToList();

                var refiltered = filtered.Where(x => x.NumericValue.HasValue).ToDictionary(x => x.Name, x => x);

                if (refiltered.Count == facts.Count)
                {
                    result = resolver.Evaluate(Value, refiltered);
                    preparedFact.HistoricalData.Add(period, (decimal)result);
                }

            }

            return preparedFact;

        }

        internal void SetValues(ConceptDefinition def)
        {
            ShortLabel = ShortLabel ?? def.Description;
            Label = Label ?? def.Description;
            Narrative = Narrative ?? def.Narrative;
        }

        internal void SetValues(Concept concept)
        {
            ShortLabel = concept.ShortLabel ?? ShortLabel ?? Label;
            Narrative = concept.Narrative ?? Narrative;
            Label = concept.Label ?? Label;
        }
    }
}
