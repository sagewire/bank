using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
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


        public static DateTime FromMillisecondsSince1970(this int milliseconds)
        {
            return ((long)milliseconds).FromMillisecondsSince1970();
        }

        public static DateTime FromMillisecondsSince1970(this long milliseconds)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddMilliseconds(milliseconds);
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

        public static string Age(this DateTime start)
        {
            return Age(start, DateTime.Now, true, false);
        }
        public static string Age(this DateTime start, bool pluralize = true, bool spellNumbers = true)
        {
            return Age(start, DateTime.Now, pluralize, spellNumbers);
        }

        public static string Age(this DateTime start, DateTime end, bool pluralize = true, bool spellNumbers = true)
        {
            var ts = end - start;
            var diff = (int)DateAndTime.DateDiff(DateInterval.Month, start, end);
            var inYears = (int)(diff / 12);

            if (ts.Days <= 31)
            {   //this record has been around less than a month

                if (ts.Days > 0)
                {
                    return string.Format("{0} {1}", spellNumbers ? ts.Days.ToLongString().ToLower() : ts.Days.ToString(),
                        utilities.Misc.Pluralizer(ts.Days, "day", "days"));
                }
                else
                {
                    return string.Format("{0} {1}", spellNumbers ? ts.Hours.ToLongString().ToLower() : ts.Hours.ToString(),
                        utilities.Misc.Pluralizer(ts.Hours, "hour", "hours"));
                }
            }

            var diffString = diff < 12 ?
                utilities.Misc.Pluralizer(diff, "month", pluralize ? "months" : "month") :
                utilities.Misc.Pluralizer(inYears, "year", pluralize ? "years" : "year");

            var diff2 = (diff < 12 ? diff : inYears);

            return string.Format("{0} {1}", spellNumbers ? diff2.ToLongString().ToLower() : diff2.ToString(), diffString);
        }
    }
}
