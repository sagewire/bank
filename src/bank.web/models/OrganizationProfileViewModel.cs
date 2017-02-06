using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.enums;
using bank.extensions;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public class OrganizationProfileViewModel //: IReports
    {
        public Organization Organization { get; set; }
        //public HeaderViewModel Header { get; set; }

        //public Report PrimaryChart { get; set; }
        //public Report SecondaryCharts { get; set; }

        //public Report SidebarCharts { get; set; }

        //public Report HighlightTable { get; internal set; }
        //public Report PieCharts { get; internal set; }

        public IList<ReportListViewModel> RawReports { get; set; } = new List<ReportListViewModel>();

        public string Title
        {
            get
            {
                return Organization.Name;
            }
        }

        //public IList<Report> Reports
        //{
        //    get
        //    {
        //        return new Report[]
        //        {
        //                    PrimaryChart,
        //                    SecondaryCharts,
        //                    SidebarCharts,
        //                    PieCharts,
        //                    HighlightTable,
        //                    DepositComposition
        //        };
        //    }
        //}

        //public Report DepositComposition { get; internal set; }

        //public IList<Column> Columns { get; set; } = new List<Column>();

    }

    public class ReportListViewModel
    {
        public DateTime Period { get; set; }
        public List<ReportTypes> ReportsAvailable { get; set; } = new List<ReportTypes>();
    }

}