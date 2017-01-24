using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.enums;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public class ReportNavigationViewModel
    {
        public Organization Organization { get; set; }
        public DateTime Period { get; set; }
        public Report Report { get; set; }
        public bool IsCurrentPeriod { get; set; }
        public HorizontalAlignment Align { get; set; } = HorizontalAlignment.Left;
        
        public bool NeedsNavigation
        {
            get
            {
                return Report.Section.Where(x => !string.IsNullOrWhiteSpace(x.Key)).Count() > 1;
            }
        }
    }
}