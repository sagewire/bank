﻿using Newtonsoft.Json;

namespace bank.reports.charts
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SeriesData
    {
        [JsonProperty(PropertyName = "zIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int? zIndex { get; set; }

        [JsonIgnore]
        public Column Column { get; set; }
        [JsonIgnore]
        public ChartConfig Chart { get; set; }

        [JsonProperty(PropertyName = "type")]
        public virtual SeriesTypes SeriesType { get; set; }

        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Name { get; set; }

        [JsonIgnore]
        public virtual bool HasFacts { get; } = true;

        public virtual bool IsRange
        {
            get
            {
                return false;
            }
        }

        public Series Series { get; internal set; }

        [JsonProperty(PropertyName = "visible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Visible { get; internal set; }

        [JsonProperty(PropertyName = "focus", NullValueHandling = NullValueHandling.Ignore)]
        public string Focus { get; internal set; }

        public SeriesData(SeriesTypes type)
        {
            SeriesType = type;
        }

        public virtual void Init()
        {

        }
    }
}