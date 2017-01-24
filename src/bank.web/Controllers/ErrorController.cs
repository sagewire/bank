using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace bank.web.Controllers
{
    public class ErrorController : ApplicationController
    {

        public ActionResult Error()
        {
            return View();
        }

    }
}