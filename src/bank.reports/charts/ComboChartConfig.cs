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
        public ComboChartConfig()
        {
            ChartType = ChartTypes.Combo;
        }

        
        public IList<Series> Series { get; set; }

        public override IList<Column> VisibleColumns
        {
            get
            {
                var list = new List<Column>();
                foreach (var series in Series)
                {
                    var counter = 0;
                    foreach (var column in Columns)
                    {
                        var addColumn = true;

                        if (series.ColumnIndex.HasValue && series.ColumnIndex.Value != counter)
                        {
                            addColumn = false;
                        }


                        if (series.ColumnStart.HasValue && series.ColumnStart > counter)
                        {
                            addColumn = false;
                        }

                        if (addColumn)
                        {
                            list.Add(column);
                        }

                        counter++;
                    }
                }
                return list;
            }
        }

        public override IList<SeriesData> GetSeriesData(Column column)
        {
            return GetSeriesData(new Column[] { column });
            //var list = new List<SeriesData>();

            //foreach (var series in Series)
            //{
            //    var seriesData = series.GetSeriesData(this, column);

            //    if (seriesData == null) continue;

            //    list.Add(seriesData);

            //    if (seriesData.IsRange)
            //    {
            //        var rangeData = seriesData as RangeSeriesData;
            //        list.Add(rangeData.PointSeriesData);
            //    }
            //}

            //return list;
        }

        public override IList<SeriesData> GetSeriesData(IList<Column> columns)
        {
            var list = new List<SeriesData>();

            foreach (var series in Series)
            {
                var counter = 0;

                foreach (var column in columns)
                {
                    var addColumn = true;

                    if (series.ColumnIndex.HasValue && series.ColumnIndex.Value != counter)
                    {
                        addColumn = false;
                    }


                    if (series.ColumnStart.HasValue && series.ColumnStart > counter)
                    {
                        addColumn = false;
                    }

                    if (addColumn)
                    {
                        var seriesData = series.GetSeriesData(this, column);

                        if (seriesData == null) continue;

                        list.Add(seriesData);

                        if (seriesData.IsRange)
                        {
                            var rangeData = seriesData as RangeSeriesData;
                            list.Add(rangeData.PointSeriesData);
                        }
                    }

                    counter++;

                    if (series.Type == SeriesTypes.Pie)
                    {   //we can only have one column in a piechart
                        break;
                    }
                }
            }

            return list;
        }


        protected override void Parse(XElement element)
        {
            base.Parse(element);

            Series = new List<charts.Series>();
            var seriesElements = element.Elements("series");

            foreach (var seriesElement in seriesElements)
            {
                var type = (SeriesTypes)Enum.Parse(typeof(SeriesTypes),
                                seriesElement.SafeAttributeValue("type"), true);

                var series = new charts.Series(type);
                series.ColumnIndex = seriesElement.SafeLongAttributeValue("column");
                series.ColumnStart = seriesElement.SafeLongAttributeValue("column-start");
                series.zIndex = (int?)seriesElement.SafeLongAttributeValue("z-index");

                var concepts = seriesElement.Elements("concept");

                foreach (var conceptElement in concepts)
                {
                    var name = conceptElement.SafeAttributeValue("name");
                    var placeholder = conceptElement.SafeAttributeValue("placeholder");

                    if (!string.IsNullOrWhiteSpace(placeholder))
                    {
                        name = Placeholders[placeholder];
                    }

                    var concept = new Concept(name);
                    series.Concepts.Add(concept);
                    Concepts.Add(concept);
                }


                //Concepts.AddRange(series.Concepts);

                Series.Add(series);

            }
        }
    }
}
