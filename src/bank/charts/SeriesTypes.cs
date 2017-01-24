﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bank.charts
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SeriesTypes
    {
        None,
        [EnumMember(Value = "pie")]
        Pie,
        [EnumMember(Value = "area")]
        Area,
        [EnumMember(Value = "areaspline")]
        AreaSpline,
        [EnumMember(Value = "line")]
        Line,
        [EnumMember(Value = "column")]
        Column,
        [EnumMember(Value = "spline")]
        Spline
    }
}
