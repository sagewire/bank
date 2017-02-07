using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace bank.web.helpers
{
    public class DataResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "html", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Html { get; set; }
    }
}