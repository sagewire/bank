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
    public class ReportsController : ApplicationController
    {

        public ActionResult Viewer(string name, string id, DateTime? period, string template, string section)
        {
            var isCurrentPeriod = false;
            if (!period.HasValue)
            {
                period = DateTime.Now.LastQuarterDate();
                isCurrentPeriod = true;
            }

            var companies = new List<int>();
            companies.Add(DecodeId(id));
            
            if (Request.QueryString["c"] != null)
            {
                var companyList = Request.QueryString["c"].Split(',');

                foreach (var company in companyList)
                {
                    if (string.IsNullOrWhiteSpace(company)) continue;

                    companies.Add(DecodeId(company));
                }
            }

            //var section = "summary";

            //if (Request.QueryString["section"] != null)
            //{
            //    section = Request.QueryString["section"];
            //}

            var rootId = companies.First();

            var factRepo = new FactRepository();

            var report = new bank.reports.Report();
            report.DebugMode = Request.QueryString["debug"] != null;
            report.Template = template;
            report.Parse();
            report.CurrentSection = section;
            report.IsCurrentPeriod = isCurrentPeriod;

            var tasks = new List<Task<FactColumn>>();

            var orgRepo = new OrganizationRepository();
            var orgs = orgRepo.GetOrganizations(companies.ToArray());
            var mdrmList = report.MdrmList(report.CurrentSectionLineItem);

            //var loadDefinitionsTask = Task.Run(() =>
            //{
            //    report.LoadDefinitions();
            //});

            foreach (var company in companies)
            {
                var task = Task<FactColumn>.Run(() =>
                {
                    var facts = factRepo.GetFacts(mdrmList.ToArray(), new int[] { company }, period, period.Value.AddYears(-2));

                    var maxPeriod = facts.Max(x => x.Period).Value;

                    var factColumn = new CompanyFactColumn
                    {
                        Facts = facts.Where(x => x.Period == maxPeriod).ToDictionary(x => x.Name, x => x),
                        Organization = orgs.Single(x => x.OrganizationId == company)
                    };

                    factColumn.HeaderUrl = Url.Url(factColumn.Organization);

                    return (FactColumn)factColumn;
                });
                tasks.Add(task);
            }

            var allTasks = new List<Task>();
            //allTasks.Add(loadDefinitionsTask);
            allTasks.AddRange(tasks);

            Task.WaitAll(allTasks.ToArray());

            foreach (var task in tasks)
            {
                report.AddColumn(task.Result);
            }
            
            var model = new ReportViewModel
            {
                Period = period,
                Report = report,
                Organization = orgs.Single(x=>x.OrganizationId == DecodeId(id))
            };

            return View(model);
        }
        
    }

}