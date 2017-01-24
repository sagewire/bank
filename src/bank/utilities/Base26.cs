using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.utilities
{
    public static class Base26
    {

        private const string Clist = "abcdefghijklmnopqrstuvwxyz";
        private static readonly char[] Clistarr = Clist.ToCharArray();


        public static long Decode(string inputString)
        {
            inputString = inputString.ToLower();

            long result = 0;
            var pow = 0;

            for (var i = inputString.Length - 1; i >= 0; i--)
            {
                var c = inputString[i];
                var pos = Clist.IndexOf(c);
                if (pos > -1)
                    result += pos * (long)Math.Pow(Clist.Length, pow);
                else
                    return -1;
                pow++;
            }

            return result;

        }

        public static string Encode(int inputNumber)
        {
            return Encode((long)inputNumber);
        }

        public static string Encode(long inputNumber)
        {
            var sb = new StringBuilder();

            do
            {
                sb.Append(Clistarr[inputNumber % Clist.Length]);
                inputNumber /= Clist.Length;
            } while (inputNumber != 0);

            return Reverse(sb.ToString());
        }

        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

}
