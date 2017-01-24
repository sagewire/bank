using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.extensions;
using DapperExtensions.Mapper;

namespace bank.poco
{
    [DebuggerDisplay("{Name}")]
    public class Fact
    {
        private static Regex _mdrm = new Regex(@"(?<series>\w{4})(?<number>\d{3,4})", RegexOptions.Compiled);
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public decimal? NumericValue { get; set; }
        public DateTime? Period { get; set; }
        /// <summary>
        /// Number of quarters in the past to compare too.
        /// </summary>
        public int TrendPeriodInterval { get; set; } = 4;
        public SortedDictionary<DateTime, decimal> HistoricalData { get; set; } = new SortedDictionary<DateTime, decimal>();
        
        public string ValueFormatted
        {
            get
            {
                if (!NumericValue.HasValue)
                {
                    return Value;
                }

                if (NumericValue >= 1000)
                {
                    return (NumericValue.Value / 1000).ToString("N0");
                }
                else
                {
                    return NumericValue.Value.ToString("N2");
                }
            }
        }

        //public string Series
        //{
        //    get
        //    {
        //        var match = _mdrm.Match(Name);
        //        if (match != null)
        //        {
        //            return match.Groups["series"].Value;
        //        }
        //        return null;
        //    }
        //}

        public string Item
        {
            get
            {
                var match = _mdrm.Match(Name);
                if (match != null)
                {
                    return match.Groups["number"].Value;
                }
                return null;
            }
        }

        public bool? Trend
        {
            get
            {
                var trend = TrendRatio;

                if (trend == null)
                {
                    return null;
                }
                else if (trend > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public decimal? TrendRatio
        {
            get
            {
                if (!Period.HasValue) return null;

                var previousPeriod = Period.Value.AddQuarters(-TrendPeriodInterval);

                if (HistoricalData.ContainsKey(previousPeriod))
                {
                    var previousValue = HistoricalData[previousPeriod];
                    if (NumericValue.HasValue && previousValue > 0)
                    {
                        return (NumericValue.Value - previousValue) / previousValue;
                    }
                }

                return null;
            }
        }

        //public ConceptDefinition MdrmDefinition { get; internal set; }

        //public int? Year
        //{
        //    get
        //    {
        //        if (Period.HasValue)
        //        {
        //            return Period.Value.Year;
        //        }
        //        return null;
        //    }
        //}

        //public int? Quarter
        //{
        //    get
        //    {
        //        if (Period.HasValue)
        //        {
        //            return (int)Math.Floor((Period.Value.Month - 1) / 3.0);
        //        }
        //        return null;
        //    }
        //}


    }

    internal class FactMapper : ClassMapper<Fact>
    {
        public FactMapper()
        {
            Map(x => x.OrganizationId).Key(KeyType.NotAKey);
            AutoMap();
        }
    }
}
