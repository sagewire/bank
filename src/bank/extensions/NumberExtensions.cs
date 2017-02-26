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

        public static string ToLongString(this int number)
        {
            if (number > 99) return number.ToString();

            if (number < 20)
            {
                if (number == 0) return "Zero";
                if (number == 1) return "One";
                if (number == 2) return "Two";
                if (number == 3) return "Three";
                if (number == 4) return "Four";
                if (number == 5) return "Five";
                if (number == 6) return "Six";
                if (number == 7) return "Seven";
                if (number == 8) return "Eight";
                if (number == 9) return "Nine";
                if (number == 10) return "Ten";

                if (number == 11) return "Eleven";
                if (number == 12) return "Twelve";
                if (number == 13) return "Thirteen";
                if (number == 14) return "Fourteen";
                if (number == 15) return "Fifteen";
                if (number == 16) return "Sixteen";
                if (number == 17) return "Seventeen";
                if (number == 18) return "Eighteen";
                if (number == 19) return "Nineteen";
            }
            else
            {
                string first = bigNubmers(number);
                int mod = number % 10;

                if (mod > 0)
                {
                    string second = "";
                    second = mod.ToLongString();
                    return string.Format("{0}-{1}", first, second);
                }
                else
                {
                    return first;
                }

            }

            return number.ToString();
        }

        private static string bigNubmers(int number)
        {
            int mod = (int)(number / 10);

            if (mod == 2) return "Twenty";
            if (mod == 3) return "Thirty";
            if (mod == 4) return "Forty";
            if (mod == 5) return "Fifty";
            if (mod == 6) return "Sixty";
            if (mod == 7) return "Seventy";
            if (mod == 8) return "Eighty";
            if (mod == 9) return "Ninety";

            return null;
        }
    }
}
