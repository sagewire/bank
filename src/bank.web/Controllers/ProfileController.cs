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
            companies.AddRange(DecodeIds(c));

            var model = new OrganizationProfileViewModel();

            var orgRepo = new OrganizationRepository();
            var org = orgRepo.GetOrganization(orgId, true);


            var columns = new List<Column>();

            foreach (var companyId in companies)
            {
                columns.Add(new CompanyColumn
                {
                    OrganizationId = companyId
                });
            }

            var statepg = Global.PeerGroups[org.StatePeerGroup];

            columns.Add(new PeerGroupColumn
            {
                PeerGroup = statepg.Code,
                HeaderText = string.Format("Peer Group {0}", statepg.Code)
            });

            foreach(var pg in org.CustomPeerGroups)
            {
                columns.Add(new PeerGroupCustomColumn
                {
                    PeerGroupCustom = pg,
                    HeaderText = string.Format("Peer Group {0}", pg.PeerGroupCode)
                });
            }




            model.PrimaryChart = new Report("profile-primary", columns);
            model.PieCharts = new Report("profile-piecharts", columns);
            model.SecondaryCharts = new Report("profile-secondary", columns);
            model.SidebarCharts = new Report("profile-sidebar", columns);
            model.HighlightTable = new Report("financial-highlights", columns);

            var tasks = new List<Task>();

            //var orgTask = Task.Run(() =>
            //{
            //    var orgRepo = new OrganizationRepository();
            //    var orgs = orgRepo.GetOrganizations(companies);
            //    //var org = orgs.Single(x => x.OrganizationId == orgId);
            //    return orgs;
            //});


            var rawReportsTask = Task.Run(() => GetRawReports(orgId));
            var populateTask = Task.Run(() => Report.PopulateReports(model.Reports, columns));
            var orgTask = Task.Run(() => orgRepo.GetOrganizations(companies));

            tasks.Add(orgTask);
            tasks.Add(rawReportsTask);
            tasks.Add(populateTask);

            Task.WaitAll(tasks.ToArray());


            foreach (var column in columns)
            {
                var companyColumn = column as CompanyColumn;

                if (companyColumn != null)
                {
                    if (companies != null)
                    {
                        companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                    }
                }
            }

            model.Organization = org;
            model.RawReports = rawReportsTask.Result;
            //model.ReportsAvailable = rawReportsTask.Result;

            //model.Header = new HeaderViewModel
            //{
            //    HeaderImage = org.ProfileBanner
            //};

 
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