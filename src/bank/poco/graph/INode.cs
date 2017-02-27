using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.poco.graph
{
    public interface INode
    {
        [JsonProperty(PropertyName = "id")]
        object NodeId { get; set; }
    }
}
