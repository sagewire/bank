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
        [JsonIgnore]
        public Organization Organization { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Name { get; set; }


        [JsonProperty(PropertyName = "title")]
        public string Title
        {
            get
            {
                return Name;
            }
        }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "Organization";

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }



        [JsonProperty(PropertyName = "color", NullValueHandling = NullValueHandling.Ignore)]
        public Color Color
        {
            get
            {
                if (IsTarget)
                {
                    var color = new Color();
                    color.Background = "#ffd700";

                    return color;
                }
                else
                {
                    return null;
                }
            }
        }


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

        [JsonProperty(PropertyName = "value")]
        public long TotalAssets { get; set; }

        [JsonProperty(PropertyName = "organizationid")]
        public int OrganizationId { get; set; }

        [JsonIgnore]
        public bool IsTarget { get; internal set; }
    }

    public class Color
    {
        [JsonProperty(PropertyName = "background")]

        public string Background { get; set; }
    }
}
