using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.crawler.services.Twitter
{
    public class FriendsResponse
    {
        [JsonProperty(PropertyName = "users")]
        public List<TwitterUser> Users { get; set; }
    }
}
