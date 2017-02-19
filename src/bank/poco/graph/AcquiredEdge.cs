using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco.graph
{
    public class AcquiredEdge : IEdge
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
        public long SourceId { get; set; }


        [JsonProperty(PropertyName = "to")]
        public string Target
        {
            get
            {
                return TargetId.ToString();
            }
        }


        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonIgnore]
        public long TargetId { get; set; }

        [JsonIgnore]
        public string Key
        {
            get
            {
                return string.Format("{0}-{1}", Math.Min(SourceId, TargetId), Math.Max(SourceId, TargetId));
            }
        }
    }
}
