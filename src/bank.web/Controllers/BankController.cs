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
    public class BankController : ApplicationController
    {

        public ActionResult BankProfile(string name, string id)
        {
            var decodedId = DecodeId(id);

            var orgRepo = new OrganizationRepository();
            var factRepo = new FactRepository();

            var reports = factRepo.GetReports(decodedId);

            var org = orgRepo.Get(decodedId);

            var model = new BankProfileViewModel
            {
                Organization = org,
                Header = new HeaderViewModel
                {
                    HeaderImage = org.ProfileBanner
                }
            };

            //model.PrimaryChart = new Report("profile-primary");
            //model.PieChartReport = new Report("profile-piecharts");
            //model.SecondaryCharts = new Report("profile-secondary");
            //model.SidebarCharts = new Report("profile-sidebar");
            //model.HighlightTable = new Report("financial-highlights");

            model.HighlightTable.Parse();
            
            ViewBag.Title = model.Title;
            return View("profile", model);

        }

    }

}