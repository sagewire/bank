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
    public class DashboardController : ApplicationController
    {
        public ActionResult Index()
        {
            return View();
        }
        //public ActionResult Viewer(string name, string id, DateTime? period, string template, string section, string c = null)
        //{
        //    var orgId = DecodeId(id);
        //    var companies = new List<int>();
        //    companies.Add(orgId);
        //    var model = new ReportViewModel();
        //    companies.AddRange(DecodeIds(c));

        //    var orgRepo = new OrganizationRepository();
        //    model.Organization = orgRepo.GetOrganization(orgId, true, true);

        //    model.Report = new Report(template, section: section);


        //    var tasks = new List<Task>();

        //    var modelTask = Task.Run(() => PopulateReportsAndColumns(model.Organization, companies, model));

        //    tasks.Add(modelTask);

        //    Task.WaitAll(tasks.ToArray());
            
        //    return View(model);


        //}

    }

}