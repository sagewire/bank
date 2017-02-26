using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco
{
    public class TimelineItem
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
        [JsonProperty(PropertyName = "start")]
        public DateTime Start { get; set; }
        [JsonProperty(PropertyName = "end")]
        public DateTime? End { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

    }
}
