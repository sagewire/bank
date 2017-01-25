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
            var isCurrentPeriod = false;
            if (!period.HasValue)
            {
                period = DateTime.Now.LastQuarterDate();
                isCurrentPeriod = true;
            }

            var orgId = DecodeId(id);
            var companies = new List<int>();
            companies.Add(orgId);
            companies.AddRange(DecodeIds(c));

            var model = new ReportViewModel();
            model.Companies = c;

            var columns = new List<Column>();

            foreach (var companyId in companies)
            {
                columns.Add(new CompanyColumn
                {
                    OrganizationId = companyId
                });
            }

            model.Report = new Report(template, columns, section: section);
            model.Report.IsCurrentPeriod = isCurrentPeriod;
            model.Report.Period = period.Value;

            var tasks = new List<Task>();

            var orgTask = Task.Run(() =>
            {
                var orgRepo = new OrganizationRepository();
                var orgs = orgRepo.GetOrganizations(companies);
                return orgs;
            });


            var populateTask = Task.Run(() =>
            {
                Report.PopulateReport(model.Report);
            });

            tasks.Add(orgTask);
            tasks.Add(populateTask);

            Task.WaitAll(tasks.ToArray());

            foreach (var column in columns)
            {
                var companyColumn = column as CompanyColumn;
                if (companies != null)
                {
                    companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                }
            }

            var org = orgTask.Result.Single(x => x.OrganizationId == orgId);
            model.Organization = org;

            //ViewBag.Title = model.Title;
            return View(model);

            //var factRepo = new FactRepository();

            //var report = new bank.reports.Report();
            //report.Template = template;
            //report.Parse();
            //report.CurrentSection = section;
            //report.IsCurrentPeriod = isCurrentPeriod;

            //var tasks = new List<Task<FactColumn>>();

            //var orgRepo = new OrganizationRepository();
            //var orgs = orgRepo.GetOrganizations(companies.ToArray());
            //var mdrmList = report.MdrmList(report.CurrentSectionLineItem);



            //foreach (var company in companies)
            //{
            //    var task = Task<FactColumn>.Run(() =>
            //    {
            //        var facts = factRepo.GetFacts(mdrmList.ToArray(), new int[] { company }, period, period.Value.AddYears(-2));

            //        var maxPeriod = facts.Max(x => x.Period).Value;

            //        var factColumn = new CompanyFactColumn
            //        {
            //            Facts = facts.Where(x => x.Period == maxPeriod).ToDictionary(x => x.Name, x => x),
            //            Organization = orgs.Single(x => x.OrganizationId == company)
            //        };

            //        factColumn.HeaderUrl = Url.Url(factColumn.Organization);

            //        return (FactColumn)factColumn;
            //    });
            //    tasks.Add(task);
            //}

            //var allTasks = new List<Task>();
            ////allTasks.Add(loadDefinitionsTask);
            //allTasks.AddRange(tasks);

            //Task.WaitAll(allTasks.ToArray());

            //foreach (var task in tasks)
            //{
            //    report.AddColumn(task.Result);
            //}

            //var model = new ReportViewModel
            //{
            //    Period = period,
            //    Report = report,
            //    Organization = orgs.Single(x=>x.OrganizationId == DecodeId(id))
            //};

            //return View(model);
        }

    }

}