using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.extensions;

namespace bank.web.helpers
{
    public static class General
    {
        public static string TrendingClass(Fact fact)
        {
            var result = "";
            if (fact == null ||
                !fact.TrendRatio.HasValue)
                return null;

            switch (fact.Trend)
            {
                case true:
                    result = "trending-up-{0}x";
                    break;
                case false:
                    result = "trending-down-{0}x";
                    break;
            }

            var multiplier = (int)Math.Floor(Math.Abs(fact.TrendRatio.Value) * 10 / 3 + 1);

            return string.Format(result, multiplier);
        }


        public static string TrendingColor(Fact fact)
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