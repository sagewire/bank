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
    [Authorize]
    public class DashboardController : ApplicationController
    {
        public ActionResult Index()
        {
            var model = new DashboardViewModel();
            model.Profile = CurrentProfile;

            var reportFactory = new ReportFactory();
            reportFactory.Template = "dashboard";
            reportFactory.Period = DateTime.Now;
            reportFactory.Organizations = model.Profile.Favorites.Take(5).ToList();
            
            if (reportFactory.Organizations.Count == 0)
            {
                reportFactory.Organizations.AddRange(model.Profile.RecentVisits.Take(5));
                reportFactory.Organizations = reportFactory.Organizations.Take(5).ToList();

            }

            model.Layout = reportFactory.Build();
            model.IsNewDashboard = !reportFactory.Organizations.Any();

            return View(model);
        }


    }

}