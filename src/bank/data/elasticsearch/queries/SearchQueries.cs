using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using bank.data.elasticsearch.responses;
using bank.poco;
using System.Text.RegularExpressions;

namespace bank.data.elasticsearch.queries
{
    public static class SearchQueries
    {
        private static Regex _bankOf = new Regex(@"bank of \w+?(?=\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static IList<Organization> Search(string name)
        {
            
            name = CleanSearchTerms(name);
            var first = name.Split(' ').FirstOrDefault();

            if (_bankOf.IsMatch(name + " "))
            {
                first = _bankOf.Match(name + " ").Value;
            }

            var template = string.IsNullOrWhiteSpace(name) ? "search-empty" : "search";

            var response = Database.ExecuteJsonResource(template, "banks", "organization", new { first = first, phrase = name });

            var result = JsonConvert.DeserializeObject<BaseResponse<object>>(response);

            var entities = result.hits.hits.Select(x => 
                                                        (Organization)JsonConvert.DeserializeObject<Organization>(x._source.ToString()))
                                           .ToList();

            return entities;
        }

        public static string CleanSearchTerms(string keywords)
        {
            return keywords.Trim();
        }
    }
}
