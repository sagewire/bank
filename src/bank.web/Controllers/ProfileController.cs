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

            var columns = new List<Column>();

            foreach (var companyId in companies)
            {
                columns.Add(new CompanyColumn
                {
                    OrganizationId = companyId
                });
            }

            model.PrimaryChart = new Report("profile-primary", columns);
            model.PieCharts = new Report("profile-piecharts", columns);
            model.SecondaryCharts = new Report("profile-secondary", columns);
            model.SidebarCharts = new Report("profile-sidebar", columns);
            model.HighlightTable = new Report("financial-highlights", columns);

            var tasks = new List<Task>();

            var orgTask = Task.Run(() =>
            {
                var orgRepo = new OrganizationRepository();
                var orgs = orgRepo.GetOrganizations(companies);
                //var org = orgs.Single(x => x.OrganizationId == orgId);
                return orgs;
            });


            var rawReportsTask = Task.Run(() =>
            {
                var factRepo = new FactRepository();
                var reports = factRepo.GetReports(orgId);
                return reports;
            });


            var populateTask = Task.Run(() =>
            {
                Report.PopulateReports(model.Reports);
            });

            tasks.Add(orgTask);
            tasks.Add(rawReportsTask);
            tasks.Add(populateTask);

            foreach(var column in columns)
            {
                var companyColumn = column as CompanyColumn;
                if (companies != null)
                {
                    companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                }
            }

            var org = orgTask.Result.Single(x => x.OrganizationId == orgId);
            model.Organization = org;
            //model.ReportsAvailable = rawReportsTask.Result;

            model.Header = new HeaderViewModel
            {
                HeaderImage = org.ProfileBanner
            };

            Task.WaitAll(tasks.ToArray());
 
            ViewBag.Title = model.Title;
            return View(model);

        }

    }

}