using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.extensions
{
    public static class NumberExtensions
    {
        private const long million = 1000000;
        private const long billion = 1000000000;
        private const long trillion = 1000000000000;

        public static string ToAbbreviatedString(this decimal number, bool inThousands = false, int roundDecimals = 1)
        {
            decimal candidate = number * (inThousands ? 1000 : 1);

            if (candidate >= trillion)
            {
                candidate = Math.Round(candidate / trillion, roundDecimals);
                return candidate.ToString() + "t";
            }
            else if (candidate >= billion)
            {
                candidate = Math.Round(candidate / billion, roundDecimals);
                return candidate.ToString() + "b";
            }
            else if (candidate >= million)
            {
                candidate = Math.Round(candidate / million, roundDecimals);
                return candidate.ToString() + "m";
            }

            return number.ToString();
        }

        public static string ToAbbreviatedString(this long number, bool inThousands = false, int roundDecimals = 1)
        {
            return ((decimal)number).ToAbbreviatedString(inThousands, roundDecimals);
        }
    }
}
