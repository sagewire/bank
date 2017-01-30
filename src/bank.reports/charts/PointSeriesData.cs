using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using bank.extensions;
using bank.poco;

namespace bank.reports.charts
{
    public class PointSeriesData : SeriesData
    {
        public PointSeriesData() : base(SeriesTypes.Bubble) { }

        public bool InXyzFormat { get; set; } = false;

        [JsonProperty(PropertyName = "data")]
        public IList<object> Data
        {
            get
            {
                var list = new List<object>();
                
                var facts = Column.GetFacts(Series.Concept.ConceptKeys);
                
                var fact = Series.Concept.PrepareFact(facts) as PeerGroupFact;

                foreach (var datum in fact.HistoricalData)
                {
                    var pgFact = (PeerGroupFact)datum.Value;

                    if (InXyzFormat)
                    {
                        list.Add(new { x = datum.Key.ToMillisecondsSince1970(), y = datum.Value.NumericValue.Value, z = pgFact.StandardDeviation });
                    }
                    else
                    {
                        list.Add(new object[] { datum.Key.ToMillisecondsSince1970(),
                            datum.Value.NumericValue.Value - (decimal)pgFact.StandardDeviation/2,
                        datum.Value.NumericValue.Value + (decimal)pgFact.StandardDeviation/2});
                    }
                }
                
                return list.ToList();
            }
        }

    }
}
