using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using bank.poco;
using bank.web.models;
using bank.extensions;

namespace bank.web.Controllers
{
    public class SearchController : ApplicationController
    {

        public ActionResult Form()
        {
            return View("modals/search");
        }


        public ActionResult Data(string q)
        {
            var entities = bank.data.elasticsearch.queries.SearchQueries.Search(q);

            var gravatarUrl = "https://www.gravatar.com/avatar/{0}?d=identicon";

            var snippets = entities.Select(x => new {
                name = x.Name,
                url = x.ProfileUrl,
                avatar = x.Avatar ?? "/assets/images/bank.png",
                assets = x.TotalAssets.HasValue ? x.TotalAssets.Value.ToAbbreviatedString(true, 0).ToUpper() : "",
                city = x.City,
                state = x.State
                //html = GetSnippet("_BankBasics", new BankBasicsViewModel { Organization = x })
            }
            );

            var model = new
            {
                status = true,
                data = new
                {
                    //entities = entities,
                    snippets = snippets
                }
            };

            var json = JsonConvert.SerializeObject(model);

            return Content(json, "application/json");
        }

        private string GetSnippet(string viewName, BankBasicsViewModel model)
        {
            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

    }
}