using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.extensions;
using Newtonsoft.Json;

namespace bank.reports.charts
{
    public class Annotation
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public DateTime? Date { get; set; }

        private long? _value = null;
        [JsonProperty(PropertyName = "value")]
        public long Value
        {
            get
            {
                if (_value.HasValue)
                {
                    return _value.Value;
                }

                return Date.Value.ToMillisecondsSince1970();
            }

        }
    }
}