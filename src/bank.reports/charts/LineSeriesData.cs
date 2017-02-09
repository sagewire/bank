using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using bank.extensions;

namespace bank.reports.charts
{
    public class LineSeriesData : SeriesData
    {
        public LineSeriesData() : base(SeriesTypes.Line) { }

        [JsonProperty(PropertyName = "data")]
        public IList<object> Data
        {
            get
            {
                var list = new Dictionary<DateTime, decimal>();

                var facts = Column.GetFacts(Series.Concept.ConceptKeys);

                if (!facts.Any())
                {
                    return new object[0];
                }

                var fact = Series.Concept.PrepareFact(facts);
                
                foreach (var datum in fact.HistoricalData)
                {
                    list.Add(datum.Key, datum.Value.NumericValue.Value);
                }

                return list.Select(x => new { x = x.Key.ToMillisecondsSince1970(), y = x.Value,
                    name = Series.Concept.Label
                }).ToList<object>();
            }
        }

    }
}
