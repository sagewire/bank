using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class ProfileHeaderViewModel
    {
        public Organization Organization { get; set; }

        public IList<ReportListViewModel> RawReports { get; set; } = new List<ReportListViewModel>();

    }
}