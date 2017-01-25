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
    public class OrganizationProfileViewModel
    {
        public Organization Organization { get; set; }
        public HeaderViewModel Header { get; set; }
        //public IList<ReportListViewModel> ReportViewModels { get; internal set; } = new List<ReportListViewModel>();

        //public IList<Fact> TotalAssets { get; internal set; }
        //public IList<Fact> TotalDeposits { get; internal set; }
        //public IList<Fact> TotalLiabilities { get; internal set; }
        //public IList<Fact> NetIncome { get; internal set; }
        //public IList<Fact> EfficiencyRatio { get; internal set; }
        //public IList<Fact> ROAA { get; internal set; }
        //public IList<Fact> ROAE { get; internal set; }

        public Report PrimaryChart { get; set; }
        public Report SecondaryCharts { get; set; }

        public Report SidebarCharts { get; set; }

        public Report HighlightTable { get; internal set; }
        public Report PieCharts { get; internal set; }

        public IList<ReportListViewModel> RawReports { get; internal set; } = new List<ReportListViewModel>();


        public IList<Report> Reports
        {
            get
            {
                return new Report[]
                {
                    PrimaryChart,
                    SecondaryCharts,
                    SidebarCharts,
                    PieCharts,
                    HighlightTable
                };
            }
        }

        public string Title
        {
            get
            {
                return Organization.Name;
            }
        }

        //public IList<Fact> ReportsAvailable { get; internal set; }

        //public string[] States
        //{
        //    get
        //    {
        //        var states = new string[]
        //        {
        //            "ak","al","ar","as","az","ca","co","ct","dc","de","fl","fm","ga","gu"
        //            ,"hi","ia","id","il","in","ks","ky","la","ma","md","me","mi","mn","mo"
        //            ,"ms","mt","nc","nd","ne","nh","nj","nm","nv","ny","oh","ok","or","pa"
        //            ,"pr","ri","sc","sd","tn","tx","ut","va","vi","vt","wa","wi","wv","wy"
        //        };

        //        var rnd = new Random();
        //        var total = rnd.Next(5, states.Length - 1);
        //        var results = new string[total];

        //        for (int i = 0; i < total; i++)
        //        {
        //            results[i] = states[rnd.Next(0, states.Length - 1)];
        //        }

        //        return results;
        //    }
        //}



        //public object[] ToSeries(IList<Fact> facts)
        //{
        //    return facts.Select(x => new
        //    {
        //        x = x.Period.Value.ToMillisecondsSince1970(),
        //        y = x.NumericValue.Value
        //    }).ToArray();
        //}
    }

    public class ReportListViewModel
    {
        public DateTime Period { get; set; }
        public List<ReportTypes> ReportsAvailable { get; set; } = new List<ReportTypes>();
    }

}