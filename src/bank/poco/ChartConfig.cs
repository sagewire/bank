using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using bank.reports;

namespace bank.poco
{
    public class ChartConfig
    {
        public string Name { get; set; }
        public SeriesType Type { get; set; }
        public IList<Series> Series { get; set; } = new List<Series>();
        public int PrimaryOrganizationId { get; set; }
        
        public static ChartConfig Build(string type)
        {
            switch(type.ToLower())
            {
                case "sankey":
                    return new SankeyChartConfig();
                default:
                    return new ChartConfig();
            }
        }

        private IEnumerable<TrendSeries> _trendSeries;
        public IEnumerable<TrendSeries> TrendSeries
        {
            get
            {
                if (_trendSeries == null)
                {
                    _trendSeries = Series.OfType<TrendSeries>();
                }
                return _trendSeries;
            }
        }
        public virtual IList<Concept> Concepts {
            get
            {
                return TrendSeries.Select(x => new Concept { MDRM = x.FactName }).Distinct().ToList();
            }
        }

        public Series PrimarySeries
        {
            get
            {
                return Series.FirstOrDefault();
            }
        }
        


    }

    public enum ChartConfigType
    {
        Trend,
        Snapshot,
        Sankey
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SeriesType
    {
        None,
        [EnumMember(Value = "pie")]
        Pie,
        [EnumMember(Value = "area")]
        Area,
        [EnumMember(Value = "areaspline")]
        AreaSpline,
        [EnumMember(Value = "line")]
        Line,
        [EnumMember(Value = "column")]
        Column,
        [EnumMember(Value = "spline")]
        Spline,
        [EnumMember(Value = "combo")]
        Combo,
        [EnumMember(Value = "sankey")]
        Sankey
    }
}
