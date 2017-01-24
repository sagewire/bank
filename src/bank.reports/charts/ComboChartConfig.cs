using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;

namespace bank.reports.charts
{
    public class ComboChartConfig : ChartConfig
    {
        public IList<Series> Series { get; set; }
        
        public override IList<SeriesData> GetSeriesData(Column column)
        {
            var list = new List<SeriesData>();

            foreach (var series in Series)
            {
                list.Add(series.GetSeriesData(this, column, series));
            }

            return list;
        }


        protected override void Parse(XElement element)
        {
            base.Parse(element);
            
            Series = new List<charts.Series>();
            var seriesElements = element.Elements("series");

            foreach(var seriesElement in seriesElements)
            {
                var type = (SeriesTypes)Enum.Parse(typeof(SeriesTypes),
                                seriesElement.SafeAttributeValue("type"), true);

                var series = new charts.Series(type);

                var concepts = seriesElement.Elements("concept");

                foreach(var conceptElement in concepts)
                {
                    var concept = new Concept(conceptElement.SafeAttributeValue("name"));
                    series.Concepts.Add(concept);
                    Concepts.Add(concept);
                }

                
                //Concepts.AddRange(series.Concepts);

                Series.Add(series);

            }
        }
    }
}
