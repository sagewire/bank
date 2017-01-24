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
using bank.data.repositories;

namespace bank.web.Controllers
{
    public class MdrmController : ApplicationController
    {

        public ActionResult Definition(string type, string id)
        {

            type = "ubpr" + type.Substring(4);

            var mdrmRepo = new MdrmDefinitionRepository();
            var factRepo = new FactRepository();
            var orgRepo = new OrganizationRepository();

            
            var def = mdrmRepo.Get(type);
            
            var model = new MdrmDefinitionViewModel
            {
                Mdrm = type,
                Definition = def
            };

            model.Chart = new ChartConfig
            {
                PrimaryOrganizationId = 5815
            };

            var companyList = DecodeIds(Request.QueryString["c"]);

            var orgs = orgRepo.GetOrganizations(companyList);

            var primaryOrg = orgs.Single(x => x.OrganizationId == companyList.First());

            model.Chart.Series.Add(new CompanyFactTrendSeries {
                Name = primaryOrg.Name,
                OrganizationId = primaryOrg.OrganizationId,
                FactName = def != null ? def.Mdrm : null,
                SeriesType = SeriesType.AreaSpline
            });


            foreach (var companyId in companyList.Skip(1))
            {
                var company = orgs.Single(x => x.OrganizationId == companyId);

                model.Chart.Series.Add(new CompanyFactTrendSeries {
                    Name = company.Name,
                    OrganizationId = company.OrganizationId,
                    FactName = type,
                    SeriesType = SeriesType.Column
                });
            }
            
            factRepo.PopulateChart(model.Chart);
            
            var html = RenderView("modals/mdrm", model);
            
            return Content(html);
        }


        //private string GetSnippet(string viewName, BankBasicsViewModel model)
        //{
        //    ViewData.Model = model;

        //    using (StringWriter sw = new StringWriter())
        //    {
        //        ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
        //        ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
        //        viewResult.View.Render(viewContext, sw);

        //        return sw.GetStringBuilder().ToString();
        //    }
        //}

    }
}