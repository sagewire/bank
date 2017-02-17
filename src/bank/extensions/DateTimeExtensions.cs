using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.extensions
{
    public static class DateTimeExtensions
    {
        public static long ToMillisecondsSince1970(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)Math.Floor(diff.TotalMilliseconds);
        }

        public static DateTime AddQuarters(this DateTime date, int quarters)
        {
            var candidate = date.AddMonths(quarters * 3);

            var daysInMonth = DateTime.DaysInMonth(candidate.Year, candidate.Month);
            
            var diff = daysInMonth - candidate.Day;

            candidate = candidate.AddDays(diff);

            return candidate;
        }

        public static int Quarter(this DateTime date)
        {
            return (int)Math.Floor((date.Month - 1) / 3.0) + 1;
        }

        public static DateTime LastQuarterDate(this DateTime date)
        {
            var candidate = date.AddMonths(-(date.Month % 3));

            var daysInMonth = DateTime.DaysInMonth(candidate.Year, candidate.Month);

            var diff = daysInMonth - candidate.Day;

            candidate = candidate.AddDays(diff);

            candidate = new DateTime(candidate.Year, candidate.Month, candidate.Day);

            return candidate;
        }
    }
}
