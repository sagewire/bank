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

        [JsonProperty(PropertyName = "source")]
        public string Source
        {
            get
            {
                return SourceId.ToString();
            }
        }

        [JsonIgnore]
        public long SourceId { get; set; }

        
        [JsonProperty(PropertyName = "target")]
        public string Target
        {
            get
            {
                return TargetId.ToString();
            }
        }

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
