﻿using System.Collections.Generic;
using bank.poco;
using bank.reports.charts;
using bank.reports.extensions;

namespace bank.reports
{
    public class TimelineElement : TemplateElement
    {
        public int? Limit { get; internal set; }
    }

}