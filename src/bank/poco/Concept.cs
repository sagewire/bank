﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.extensions;
using bank.poco;
//using bank.reports.formulas;
using bank.utilities;

namespace bank.poco
{
    [DebuggerDisplay("{Value}")]
    public class Concept
    {
        private static Regex _concepts = new Regex(@"[\w~_][\w\d]{7}", RegexOptions.Compiled);

        public string Balance { get; set; }

        public Concept(string value)
        {
            if (value == null)
            {
                throw new Exception();
            }
            value = value.ToUpper();
            Value = value;
            Name = value.ToUpper();

            MatchCollection matches = _concepts.Matches(value);

            if (matches.Count > 0)
            {
                ConceptKeys = new List<string>();

                foreach (Match match in matches)
                {


                    if (Global.Concepts.ContainsKey(value))
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

        public string ValueFormatted(Fact fact, bool showInThousands = false)
        {
            if (fact == null)
            {
                return null;
            }
            if (!fact.NumericValue.HasValue)
            {
                return Value;
            }

            var val = fact.NumericValue.Value;

            char? unit = this?.Unit ?? Unit;
            string result = null;

            if (!string.IsNullOrWhiteSpace(FormatHint))
            {
                switch(FormatHint)
                {
                    case "abbr":
                        result = val.ToAbbreviatedString().ToUpper();
                        break;
                }
            }
            else
            {
                switch (unit)
                {
                    case 'P':
                        result = val.ToString("#.###");
                        break;
                    case 'U':
                        //var format = "N0";
                        //result = Math.Round(fact.NumericValue.Value / 1000, 0).ToString(format);
                        if (showInThousands) val = val / 1000;
                        result = val.ToString("N0");
                        break;
                    case 'D':
                        if (showInThousands) val = val / 1000;
                        result = val.ToString();
                        break;
                    default:
                        // result = "na";
                        if (showInThousands) val = val / 1000;
                        
                        result = val.ToString("N0");
                        break;
                }
            }

            if (Negative.HasValue && Negative.Value)
            {
                return string.Format("({0})", result);
            }

            return result;

            //if (NumericValue >= 1000)
            //{
            //    return (NumericValue.Value / 1000).ToString("N0");
            //}
            //else
            //{
            //    return NumericValue.Value.ToString("N2");
            //}

        }

        public string Name { get; set; }
        public string Label { get; set; }
        public string Narrative { get; set; }
        public string Value { get; set; }
        public char? Unit { get; set; }
        public List<string> ConceptKeys { get; set; }
        public bool? Negative { get; set; }

        public string ItemNumber
        {
            get
            {
                return Value.SafeSubstring(4, 4);
            }
        }

        private string _shortLabel;
        public string ShortLabel {
            get
            {
                return _shortLabel ?? Label;
            }
            set
            {
                _shortLabel = value;
            }
        }

        public string FormatHint { get; set; }
        public int? Nulls { get; internal set; } = 0;
        public List<Concept> Children { get; set; }

        public Fact PrepareFact(Fact fact)
        {
            return PrepareFact(new Fact[] { fact });
        }

        public static List<string> GetConceptKeys(IList<Concept> concepts)
        {
            var conceptKeys = new List<string>();

            foreach (var concept in concepts)
            {
                conceptKeys.AddRange(concept.ConceptKeys);
            }

            return conceptKeys;
        }

        public Fact PrepareFact(IList<Fact> facts)
        {
            if (!facts.Any())
            {
                return null;
            }

            var resolver = new FactResolver();
            var factType = facts.First().FactType;

            var result = resolver.Evaluate(Value, facts.ToDictionary(x => x.Name, x => x), Nulls);

            if (result == null)
            {
                return null;
            }

            var factSample = facts.First();

            var preparedFact = Fact.Build(factType);
            preparedFact.Name = Value;
            preparedFact.NumericValue = (decimal)result;
            preparedFact.Period = factSample.Period;
            //preparedFact.Unit = factSample.Unit;
            //preparedFact.Concept = factSample.Concept;

            var peerGroupFact = preparedFact as PeerGroupFact;

            if (peerGroupFact != null && facts.Count == 1)
            {
                var sourceFact = facts.First() as PeerGroupFact;
                peerGroupFact.StandardDeviation = sourceFact.StandardDeviation;
                peerGroupFact.MinValue = sourceFact.MinValue;
                peerGroupFact.MaxValue = sourceFact.MaxValue;
                //peerGroupFact.Concept = sourceFact.Concept;
                //peerGroupFact.Unit = sourceFact.Unit;
            }

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
                var filtered = facts.Select(x => x.HistoricalData.ContainsKey(period) ?
                                          x.HistoricalData.Where(h => h.Key == period).First().Value :
                                          Fact.Build(factType)).ToList();

                var refiltered = filtered.Where(x => x.NumericValue.HasValue).ToDictionary(x => x.Name, x => x);

                if (refiltered.Count == facts.Count)
                {
                    result = resolver.Evaluate(Value, refiltered, Nulls);

                    var oldFact = Fact.Build(factType);
                    oldFact.Name = preparedFact.Name;
                    oldFact.NumericValue = (decimal)result;

                    if (facts.Count == 1 && peerGroupFact != null)
                    {
                        var oldFactPg = oldFact as PeerGroupFact;
                        var oldFactSource = filtered.First() as PeerGroupFact;

                        oldFactPg.StandardDeviation = oldFactSource.StandardDeviation;
                        oldFactPg.MinValue = oldFactSource.MinValue;
                        oldFactPg.MaxValue = oldFactSource.MaxValue;
                    }

                    preparedFact.HistoricalData.Add(period, oldFact);
                }

            }

            //preparedFact.Concept = factSample.Concept;
            //preparedFact.Unit = factSample.Unit;

            return preparedFact;

        }

        public void SetValues(ConceptDefinition def)
        {
            Name = Name ?? def.Mdrm;
            ShortLabel = ShortLabel ?? def.Description;
            Label = Label ?? def.Description;
            Narrative = Narrative ?? def.Narrative;
            Unit = Unit ?? def.Unit;
            Negative = Negative ?? def.Negative;
            Balance = Balance ?? def.Balance;
        }

        public void SetValues(Concept concept)
        {
            ShortLabel = concept.ShortLabel ?? ShortLabel ?? Label;
            Narrative = concept.Narrative ?? Narrative;
            Label = concept.Label ?? Label;
            Unit = concept.Unit ?? Unit;
            Negative = concept.Negative ?? Negative;
            Nulls = concept.Nulls?? Nulls;
            Balance = concept.Balance ?? Balance;

        }
    }
}
