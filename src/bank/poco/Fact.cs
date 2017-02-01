using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using bank.enums;
using bank.extensions;
using DapperExtensions.Mapper;

namespace bank.poco
{
    [DebuggerDisplay("{Name}")]
    public class Fact
    {
        private static Regex _mdrm = new Regex(@"(?<series>\w{4})(?<number>\d{3,4})", RegexOptions.Compiled);
        public string Name { get; set; }
        public string Value { get; set; }
        public decimal? NumericValue { get; set; }
        public DateTime? Period { get; set; }
        public char? Unit { get; set; }

        public virtual FactTypes FactType { get; }
        /// <summary>
        /// Number of quarters in the past to compare too.
        /// </summary>
        public int TrendPeriodInterval { get; set; } = 4;
        public SortedDictionary<DateTime, Fact> HistoricalData { get; set; } = new SortedDictionary<DateTime, Fact>();
        
        public static Fact Build(FactTypes type)
        {
            switch(type)
            {
                case FactTypes.Company:
                    return new Fact();
                case FactTypes.PeerGroup:
                    return new PeerGroupFact();
                default:
                    throw new NotSupportedException();
            }
        }

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
                    if (NumericValue.HasValue && previousValue.NumericValue.Value > 0)
                    {
                        return (NumericValue.Value - previousValue.NumericValue.Value) / previousValue.NumericValue.Value;
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


}
