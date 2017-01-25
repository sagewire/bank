using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;
using bank.reports.charts;

namespace bank.web.models
{
    public class LineItemViewModel
    {
        public int Level { get; set; } = 0;
        public LineItem LineItem { get; set; }
        public Report Report { get; set; }

        //public decimal[] FactHistoricalData(Fact fact)
        //{
        //    if (fact != null && fact.HistoricalData != null && fact.NumericValue.HasValue)
        //    {
        //        var facts = new List<decimal>();
        //        facts.AddRange(
        //                fact.HistoricalData
        //                    .OrderBy(x => x.Key)
        //                    .Select(x=>x.Value)
        //                    .ToList()
        //                    );

        //        facts.Add(fact.NumericValue.Value);

        //        return facts.ToArray();
        //    }
        //    return null;
        //}

        //public string SubTemplateUrl(string subTemplate)
        //{
        //    return HttpContext.Current.Request.RawUrl + "/" + subTemplate;
        //}

        public IList<SeriesData> GetSeriesData(Column column)
        {
            if (!LineItem.Concepts.Any())
            {
                return null;
            }

            var chartConfig = new ComboChartConfig();
            chartConfig.ChartType = ChartTypes.Combo;
            chartConfig.Concepts = LineItem.Concepts;
            chartConfig.Series = new List<Series>();
            chartConfig.Series.Add(new Series(SeriesTypes.AreaSpline));
            chartConfig.Series.First().Concepts = LineItem.Concepts;

            var seriesData = chartConfig.GetSeriesData(column);

            return seriesData;
        }

        public string TrendingColor(Fact fact)
        {
            if (fact != null && fact.Trend.HasValue)
            {
                string format = "background-color: rgba({0}, {1})";
                if (fact.Trend.Value)
                {
                    return string.Format(format, "0,255,0", fact.TrendRatio.Value * (decimal)0.5);
                }
                else
                {
                    return string.Format(format, "255,0,0", Math.Abs(Math.Round(fact.TrendRatio.Value * (decimal)0.5, 2)));
                }
            }
            return null;
        }
    }
}