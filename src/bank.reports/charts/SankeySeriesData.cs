using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.reports.charts
{
    public class SankeySeriesData : SeriesData
    {

        public SankeySeriesData() : base(SeriesTypes.Sankey) {
            
        }

        public new SankeyChartConfig Chart
        {
            get
            {
                return base.Chart as SankeyChartConfig;
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
        public IList<object> Data
        {
            get; set;
        }


        //[JsonProperty(PropertyName = "data")]
        //public IList<PieData> Data
        //{
        //    get
        //    {
        //        var list = new List<PieData>();

        //        foreach (var concept in Series.Concepts)
        //        {
        //            var facts = Column.GetFacts(concept.ConceptKeys);
        //            var fact = concept.PrepareFact(facts);


        //            list.Add(new PieData
        //            {
        //                y = fact.NumericValue.Value,
        //                temp = concept.Value,
        //                name = concept.ShortLabel
        //            });
        //        }

        //        return list;
        //    }
        //}


    }
}
