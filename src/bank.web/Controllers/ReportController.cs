using System;
using System.IO;
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

        public ActionResult Index(string segment)
        {
            var template = GetTemplate(segment);
            
            var model = new ReportViewModel();
            model.Profile = CurrentProfile;

            var reportFactory = new ReportFactory();
            reportFactory.Template = template;
            reportFactory.Period = DateTime.Now.LastQuarterDate();

            var companyRank = new CompanyRankColumn();
            //companyRank.OrderBy = id ?? "UBPR2170";

            reportFactory.SetColumn(companyRank);
            

            model.Layout = reportFactory.Build();
            //model.IsNewDashboard = !reportFactory.Organizations.Any();

            return View("viewer", model);
        }

        private string GetTemplate(string segments)
        {
            while(!string.IsNullOrWhiteSpace(segments))
            {
                var testFile = Path.Combine(Settings.ReportTemplatePath, segments + ".xml");

                if (System.IO.File.Exists(testFile))
                {
                    return segments;
                }

                if (!segments.Contains("/"))
                {
                    throw new FileNotFoundException(Request.RawUrl);
                }

                segments = segments.Substring(0, segments.LastIndexOf("/"));
            }

            return null;
        }

    }

}