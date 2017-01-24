using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.extensions;
using bank.poco;

namespace bank.web.models
{
    public class LineChartModel
    {
        public string ElementId { get; set; }
        public string Title { get; set; }

        public IList<Fact> Data { get; set; }

        public IList<string> Labels
        {
            get
            {
                return Data.Select(x => string.Format("\"{0} Q{1}\"", x.Period.Value.Year, x.Period.Value.Quarter())).ToList();
            }
        }
    }
}