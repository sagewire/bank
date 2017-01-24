using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.reports.charts
{
    public class PieSeriesData : SeriesData
    {

        public PieSeriesData() : base(SeriesTypes.Pie) {
            
        }

        public new ComboChartConfig Chart
        {
            get
            {
                return base.Chart as ComboChartConfig;
            }
        }

        public override string Name
        {
            get
            {
                return Chart.Title;
            }
        }

        [JsonProperty(PropertyName = "data")]
        public IList<PieData> Data
        {
            get
            {
                var list = new List<PieData>();

                foreach (var concept in Series.Concepts)
                {
                    var facts = Column.GetFacts(concept.ConceptKeys);
                    var fact = concept.PrepareFact(facts);


                    list.Add(new PieData
                    {
                        y = fact.NumericValue.Value,
                        temp = concept.Value,
                        name = concept.ShortLabel
                    });
                }
                
                return list;
            }
        }

        public class PieData
        {
            public string temp;

            public decimal y { get; set; }
            public string name { get; set; }
        }
    }
}
