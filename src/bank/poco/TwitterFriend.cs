using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco
{
    public class TwitterFriend
    {
        public int OrganizationId { get; set; }
        public string ScreenName { get; set; }
        [JsonProperty(PropertyName = "id_str")]
        public string TwitterId { get; set; }
        [JsonIgnore]
        public long Id { get; set; }
    }
}
