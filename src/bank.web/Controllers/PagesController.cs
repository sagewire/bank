using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace bank.web.Controllers
{
    public class PagesController : ApplicationController
    {
        
        public ActionResult Placeholder()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }
        
    }

}