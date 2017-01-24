using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public class ReportViewModel
    {
        public Organization Organization { get; set; }
        public DateTime? Period { get; internal set; }
        public Report Report { get; set; }

    }
}