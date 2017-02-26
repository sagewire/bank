using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class TimelineItemViewModel
    {
        public Organization Organization { get; set; }

        public TimelineItem Item { get; set; }
    }
}