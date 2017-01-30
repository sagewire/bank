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
using bank.web.helpers;

namespace bank.web.Controllers
{
    public class ReportController : ApplicationController
    {

        public ActionResult Viewer(string name, string id, DateTime? period, string template, string section, string c = null)
        {
            var orgId = DecodeId(id);
            var companies = new List<int>();
            companies.Add(orgId);
            var model = new ReportViewModel();
            companies.AddRange(DecodeIds(c));

            model.Report = new Report(template, section: section);


            var tasks = new List<Task>();

            var modelTask = Task.Run(() => PopulateReportsAndColumns(orgId, companies, model));

            tasks.Add(modelTask);

            Task.WaitAll(tasks.ToArray());
            
            return View(model);

            //var isCurrentPeriod = false;
            //if (!period.HasValue)
            //{
            //    period = DateTime.Now.LastQuarterDate();
            //    isCurrentPeriod = true;
            //}

            //var orgId = DecodeId(id);
            //var companies = new List<int>();
            //companies.Add(orgId);
            //companies.AddRange(DecodeIds(c));

            //var model = new ReportViewModel();
            //model.Companies = c;

            //var columns = new List<Column>();

            //foreach (var companyId in companies)
            //{
            //    columns.Add(new CompanyColumn
            //    {
            //        OrganizationId = companyId
            //    });
            //}

            //model.Report = new Report(template, columns, section: section);
            //model.Report.IsCurrentPeriod = isCurrentPeriod;
            //model.Report.Period = period.Value;

            //var tasks = new List<Task>();

            //var orgTask = Task.Run(() =>
            //{
            //    var orgRepo = new OrganizationRepository();
            //    var orgs = orgRepo.GetOrganizations(companies);
            //    return orgs;
            //});


            //var populateTask = Task.Run(() =>
            //{
            //    Report.PopulateReport(model.Report);
            //});

            //tasks.Add(orgTask);
            //tasks.Add(populateTask);

            //Task.WaitAll(tasks.ToArray());

            //foreach (var column in columns)
            //{
            //    var companyColumn = column as CompanyColumn;
            //    if (companies != null)
            //    {
            //        companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
            //    }
            //}

            //var org = orgTask.Result.Single(x => x.OrganizationId == orgId);
            //model.Organization = org;

            //return View(model);

        }

    }

}