using Newtonsoft.Json;

namespace bank.reports.charts
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SeriesData
    {
        [JsonIgnore]
        public Column Column { get; set; }
        [JsonIgnore]
        public ChartConfig Chart { get; set; }

        [JsonProperty(PropertyName = "type")]
        public SeriesTypes SeriesType { get; set; }

        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Name
        {
            get
            {
                return Series.Concept.Label;
            }
        }
        
        public Series Series { get; internal set; }

        public SeriesData(SeriesTypes type)
        {
            SeriesType = type;
        }

        public virtual void Init()
        {
            
        }
    }
}