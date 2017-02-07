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
using Microsoft.AspNet.Identity;
using bank.web.helpers;

namespace bank.web.Controllers
{
    public class ProfileController : ApplicationController
    {

        //public ActionResult Organization(string name, string id, string c = null)
        //{
        //    var orgId = DecodeId(id);
        //    var companies = new List<int>();
        //    companies.Add(orgId);
        //    var model = new OrganizationProfileViewModel();
        //    companies.AddRange(DecodeIds(c));

        //    var orgRepo = new OrganizationRepository();
        //    model.Organization = orgRepo.GetOrganization(orgId, true, true);

        //    model.PrimaryChart = new Report("profile-primary");
        //    model.PieCharts = new Report("profile-piecharts");
        //    model.SecondaryCharts = new Report("profile-secondary");
        //    model.SidebarCharts = new Report("profile-sidebar");
        //    model.HighlightTable = new Report("financial-highlights");
        //    model.DepositComposition = new Report("deposit-composition");

        //    var tasks = new List<Task>();

        //    var modelTask = Task.Run(() => PopulateReportsAndColumns(model.Organization, companies, model));
        //    var rawReportsTask = Task.Run(() => RawReports(model.Organization));

        //    tasks.Add(rawReportsTask);
        //    tasks.Add(modelTask);

        //    Task.WaitAll(tasks.ToArray());

        //    model.RawReports = rawReportsTask.Result;

        //    ViewBag.Title = model.Title;
        //    return View(model);

        //}

        private IList<ReportListViewModel> RawReports(Organization org)
        {
            var list = new Dictionary<DateTime, ReportListViewModel>();

            foreach (var report in org.ReportImports)
            {
                ReportListViewModel item = null;
                if (list.ContainsKey(report.Period))
                {
                    item = list[report.Period];
                }
                else
                {
                    item = new ReportListViewModel();
                    item.Period = report.Period;
                    list.Add(item.Period, item);
                }
                item.ReportsAvailable.Add(report.ReportType);

            }

            return list.OrderByDescending(x => x.Key).Select(x => x.Value).ToList();

        }
        
        [ClaimsAuthorize(MemberType = "free")]
        public ActionResult Viewer(string name, string id, DateTime? period, string section, string c = null, string template = "profile-layout")
        {
            var orgId = DecodeId(id);
            var companies = new List<int>();
            companies.Add(orgId);
            var model = new ReportViewModel();
            companies.AddRange(DecodeIds(c));

            var orgRepo = new OrganizationRepository();
            model.Organization = orgRepo.GetOrganization(orgId, true, true);
            
            model.Layout = new Layout();
            model.Layout.Load(template);

            PopulateReportsAndColumns(model.Organization, companies, model.Layout, period);

            SetProfileVisit(orgId);

            ViewBag.Title = model.Title;
            return View(model);

        }

    }

}