using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bank.poco
{
    public class Series
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonIgnore]
        public IList<DateTime> X { get; set; }
        [JsonIgnore]
        public IList<decimal> Y { get; set; }

        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SeriesType SeriesType { get; set; }

        [JsonProperty(PropertyName = "data")]
        public IList<object[]> Data
        {
            get
            {
                var pairs = new List<object[]>();

                for(int i = 0; i < X.Count; i++)
                {
                    var pair = new object[]
                    {
                        X[i],
                        Y[i]
                    };

                    pairs.Add(pair);
                }

                return pairs;
            }
        }

        public Fact Fact { get; internal set; }
    }
}
