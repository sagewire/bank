using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.data.elasticsearch.responses
{

    public class BaseResponse<T>
    {
        public int took { get; set; }
        public bool time_out { get; set; }
        public HitsSummary<T> hits { get; set; }
    }

    public class HitsSummary<T>
    {
        public int total { get; set; }
        public double? max_score { get; set; }
        public List<BaseHit<T>> hits { get; set; }

    }

    public class BaseHit<T>
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }

        public T _source { get; set; }
    }

    public class Aggregation<T> where T : BaseBucket
    {
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
        public List<T> buckets { get; set; }
    }

    public class BaseBucket
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }
}
