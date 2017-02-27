using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco.graph
{
    public class Edge : IEdge
    {

        [JsonProperty(PropertyName = "from")]
        public string Source
        {
            get
            {
                return SourceId.ToString();
            }
        }

        [JsonIgnore]
        public object SourceId { get; set; }


        [JsonProperty(PropertyName = "to")]
        public string Target
        {
            get
            {
                return TargetId.ToString();
            }
        }


        [JsonProperty(PropertyName = "label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonIgnore]
        public object TargetId { get; set; }

        [JsonIgnore]
        public string Key
        {
            get
            {
                var items = new List<string> { SourceId.ToString(), TargetId.ToString() };
                items.Sort();

                return string.Format("{0}-{1}", items[0], items[1]);
            }
        }
    }
}
