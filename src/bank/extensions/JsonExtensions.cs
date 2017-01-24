using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bank.extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return json;
        }

        public static string ToJson(this List<object> list)
        {
            var json = JsonConvert.SerializeObject(list);
            return json;
        }
        
    }
}
