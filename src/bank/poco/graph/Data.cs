using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco.graph
{
    public class Data
    {
        [JsonProperty(PropertyName = "nodes")]
        public List<INode> Nodes { get; set; } = new List<INode>();

        [JsonProperty(PropertyName = "edges")]
        public List<IEdge> Edges { get; set; } = new List<IEdge>();
    }
}
