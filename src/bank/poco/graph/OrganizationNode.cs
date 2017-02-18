using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco.graph
{
    public class OrganizationNode : INode
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "Organization";

        [JsonProperty(PropertyName = "id")]
        public string Node
        {
            get
            {
                return NodeId.ToString();
            }
        }

        [JsonIgnore]
        public long NodeId { get; set; }

        [JsonProperty(PropertyName = "organizationid")]
        public int OrganizationId { get; set; }
    }
}
