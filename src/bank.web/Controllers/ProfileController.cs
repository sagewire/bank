using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.poco;
using bank.reports;
using bank.web.models;
using bank.extensions;
using bank.enums;
using bank.reports.formulas;

namespace bank.web.Controllers
{
    public class ProfileController : ApplicationController
    {

        public ActionResult Organization(string name, string id, string c = null)
        {
            var orgId = DecodeId(id);
            var companies = new List<int>();
            companies.Add(orgId);
            var model = new OrganizationProfileViewModel();
            companies.AddRange(DecodeIds(c));

            
            model.PrimaryChart = new Report("profile-primary");
            model.PieCharts = new Report("profile-piecharts");
            model.SecondaryCharts = new Report("profile-secondary");
            model.SidebarCharts = new Report("profile-sidebar");
            model.HighlightTable = new Report("financial-highlights");

            var tasks = new List<Task>();

            var modelTask = Task.Run(()=> PopulateReportsAndColumns(orgId, companies, model));
            //var rawReportsTask = Task.Run(() => GetRawReports(orgId));
            
            //tasks.Add(rawReportsTask);
            tasks.Add(modelTask);

            Task.WaitAll(tasks.ToArray());

            model.RawReports = RawReports(model.Organization);
 
            ViewBag.Title = model.Title;
            return View(model);

        }

        private IList<ReportListViewModel> RawReports(Organization org)
        {
            var list = new Dictionary<DateTime, ReportListViewModel>();

            foreach (var report in org.ReportImports)
            {
                ReportListViewModel item = null;
                if (list.ContainsKey(report.Quarter))
                {
                    item = list[report.Quarter];
                }
                else
                {
                    item = new ReportListViewModel();
                    item.Period = report.Quarter;
                    list.Add(item.Period, item);
                }
                item.ReportsAvailable.Add(report.ReportType);

            }

            return list.OrderByDescending(x => x.Key).Select(x => x.Value).ToList();

        }

    }

}