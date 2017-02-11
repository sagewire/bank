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
        

        public ActionResult Viewer(string name, string id, DateTime? period, string section, string c = null, string template = "profile-layout")
        {
            //Response.CacheControl = HttpCacheability.Public.ToString();
            //Response.Expires = Settings.PageCacheMinutes;

            Response.CacheControl = HttpCacheability.Private.ToString();
            Response.Cache.SetMaxAge(new TimeSpan(0, 0, 10));

            var orgId = DecodeId(id);
            var companies = new List<int>();
            companies.Add(orgId);
            companies.AddRange(DecodeIds(c));

            var orgRepo = new OrganizationRepository();
            var org = orgRepo.GetOrganization(orgId, true, true);

            //var periodStart = org.ReportImports.Select(x => x.Period).Min();
            var periodEnd = period.HasValue ? period.Value : org.ReportImports.Select(x => x.Period).Max();

            var reportFactory = new ReportFactory();
            reportFactory.Template = template;
            reportFactory.SectionFilter = section;
            reportFactory.Period = periodEnd;
            reportFactory.OrganizationIds = companies;

            if (!string.IsNullOrWhiteSpace(org.StatePeerGroup))
            {
                if (Global.PeerGroups.ContainsKey(org.StatePeerGroup))
                {
                    reportFactory.PeerGroups.Add(Global.PeerGroups[org.StatePeerGroup]);
                }
            }
            reportFactory.CustomPeerGroups.AddRange(org.CustomPeerGroups);

            var layout = reportFactory.Build();

            var model = new ReportViewModel();
            model.Layout = layout;
            model.Organization = org;
            model.Profile = CurrentProfile;
            
            SetProfileVisit(model.Organization.OrganizationId);

            ViewBag.Title = model.Title;

            return View(model);

        }

    }

}