using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.crawler.services.Twitter
{
    public class TwitterUser
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Url { get; set; }

        [JsonProperty(PropertyName = "listed_count")]
        public int ListedCount { get; set; }

        [JsonProperty(PropertyName = "followers_count")]
        public int FollowersCount { get; set; }
        public bool Verified { get; set; }
        public string Description { get; set; }

        [JsonProperty(PropertyName = "statuses_count")]
        public int StatusesCount { get; set; }

        [JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; set; }
    }
}
