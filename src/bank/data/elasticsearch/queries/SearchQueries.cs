using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using bank.data.elasticsearch.responses;
using bank.poco;

namespace bank.data.elasticsearch.queries
{
    public static class SearchQueries
    {
        public static IList<Organization> Search(string name)
        {
            name = CleanSearchTerms(name);
            var response = Database.ExecuteJsonResource("search", "banks", "organization", new { name = name });

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
