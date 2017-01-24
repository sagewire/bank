using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bank.reports.charts
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChartTypes
    {
        [EnumMember(Value = "combo")]
        Combo,
        [EnumMember(Value = "sankey")]
        Sankey
    }
}
