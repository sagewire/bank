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




    }
}