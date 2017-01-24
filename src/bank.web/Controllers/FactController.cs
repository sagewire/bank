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

namespace bank.web.Controllers
{
    public class FactController : ApplicationController
    {

        public ActionResult Details(string id, string companies)
        {
            var factRepo = new FactRepository();
            var model = new FactViewModel();
            
            model.CompanyIds = DecodeIds(companies);
            model.Facts = factRepo.GetFacts(new string[] { id }, model.CompanyIds).ToList();
            model.RootCompanyId = model.CompanyIds.First();

            return View(model);
        }

    }

}