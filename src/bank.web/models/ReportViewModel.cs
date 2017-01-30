using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public class ReportViewModel : IReports
    {
        public Organization Organization { get; set; }
        public DateTime? Period { get; internal set; }
        public Report Report { get; set; }

        public string Companies { get; set; }

        public IList<Report> Reports
        {
            get
            {
                return new Report[] { Report };
            }
        }
    }
}