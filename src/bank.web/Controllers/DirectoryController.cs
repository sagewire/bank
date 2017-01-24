using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.poco;
using bank.reports;
using bank.web.models;
using bank.crawl;

namespace bank.web.Controllers
{
    public class DirectoryController : ApplicationController
    {

        public ActionResult Index(string id)
        {
            var model = new BanksViewModel();

            var banks = Repository<Organization>.New()
                            .All()
                            .OrderByDescending(x=>x.TotalAssets)
                            .OrderByDescending(x=>x.Avatar != null)
                            .Take(40)
                            .ToList();

            model.Banks = banks;

            return View(model);
        }

    }

}