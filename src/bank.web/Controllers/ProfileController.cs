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
            var rawReportsTask = Task.Run(() => GetRawReports(orgId));
            
            tasks.Add(rawReportsTask);
            tasks.Add(modelTask);

            Task.WaitAll(tasks.ToArray());

            model.RawReports = rawReportsTask.Result;
 
            ViewBag.Title = model.Title;
            return View(model);

        }

        private IList<ReportListViewModel> GetRawReports(int orgId)
        {
            var factRepo = new FactRepository();
            var reports = factRepo.GetReports(orgId);

            var list = new Dictionary<DateTime, ReportListViewModel>();

            foreach (var report in reports)
            {
                ReportListViewModel item = null;
                if (list.ContainsKey(report.Period.Value))
                {
                    item = list[report.Period.Value];
                }
                else
                {
                    item = new ReportListViewModel();
                    item.Period = report.Period.Value;
                    list.Add(item.Period, item);
                }
                item.ReportsAvailable.Add(ReportType.Parse(report.Name));

            }

            return list.OrderByDescending(x => x.Key).Select(x => x.Value).ToList();

        }

    }

}