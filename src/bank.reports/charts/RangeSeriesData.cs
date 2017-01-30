using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using bank.extensions;

namespace bank.reports.charts
{
    public class RangeSeriesData : LineSeriesData
    {
        private SeriesTypes _originalSeriesType = SeriesTypes.None;

        public override SeriesTypes SeriesType
        {
            get
            {
                return SeriesTypes.Line;
            }

            set
            {
                if (value.ToString().ToLower().Contains("range"))
                {
                    _originalSeriesType = value;
                }
                base.SeriesType = value;
            }
        }

        public override bool IsRange
        {
            get
            {
                return true;
            }
        }

        public PointSeriesData PointSeriesData
        {
            get
            {
                var seriesData = new PointSeriesData
                {
                    Chart = this.Chart,
                    Column = this.Column,
                    SeriesType = _originalSeriesType,
                    Series = this.Series
                };

                return seriesData;
            }
        }



        //[JsonProperty(PropertyName = "data")]
        //public IList<object[]> Data
        //{
        //    get
        //    {
        //        var list = new Dictionary<DateTime, decimal>();

        //        var facts = Column.GetFacts(Series.Concept.ConceptKeys);
        //        var fact = Series.Concept.PrepareFact(facts);

        //        foreach (var datum in fact.HistoricalData)
        //        {
        //            list.Add(datum.Key, datum.Value.NumericValue.Value);
        //        }

        //        return list.Select(x => new object[] { x.Key.ToMillisecondsSince1970(), x.Value }).ToList();
        //    }
        //}

    }
}
