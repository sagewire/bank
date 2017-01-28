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

        [JsonProperty(PropertyName = "data")]
        public IList<object> Data
        {
            get
            {
                var list = new Dictionary<DateTime, decimal>();
                
                var facts = Column.GetFacts(Series.Concept.ConceptKeys);
                var peerGroupFact = facts.First() as PeerGroupFact;

                var fact = Series.Concept.PrepareFact(facts);

                foreach (var datum in fact.HistoricalData)
                {
                    list.Add(datum.Key, datum.Value);
                }
                
                return list.Select(d => (object)new { x = d.Key.ToMillisecondsSince1970(), y = d.Value, z = peerGroupFact.StandardDeviation }).ToList();
            }
        }

    }
}
