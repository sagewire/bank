using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class DashboardViewModel
    {
        public bool IsNewDashboard { get; internal set; }
        public Layout Layout { get; internal set; }
        public AppUser Profile { get; internal set; }
    }
}