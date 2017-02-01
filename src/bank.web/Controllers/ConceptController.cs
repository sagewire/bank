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
using bank.reports.charts;

namespace bank.web.Controllers
{
    public class ConceptController: ApplicationController
    {

        public ActionResult Definition(string type, string id, string c)
        {
            var originalType = type;
            type = "ubpr" + type.Substring(4);

            var mdrmRepo = new ConceptDefinitionRepository();
            var factRepo = new FactRepository();
            var orgRepo = new OrganizationRepository();



            var def = mdrmRepo.Get(type);

            var model = new ConceptDefinitionViewModel
            {
                Mdrm = type,
                Definition = def
            };
            

            var companyList = DecodeIds(c);

            var orgs = orgRepo.GetOrganizations(companyList);

            var primaryOrg = orgs.Single(x => x.OrganizationId == companyList.First());

            var columns = new List<Column>();

            foreach (var companyId in companyList)
            {
                var company = orgs.Single(x => x.OrganizationId == companyId);

                var column = new CompanyColumn
                {
                    OrganizationId = companyId,
                    Organization = company
                };

                columns.Add(column);
                
            }


            var placeholders = new Dictionary<string, string>();
            placeholders.Add("primary", originalType);
            placeholders.Add("secondary", originalType);

            var report = new Report("concept-definition", columns, placeholders);

            var periodStart = primaryOrg.ReportImports.Select(x => x.Period).Min();
            var periodEnd = primaryOrg.ReportImports.Select(x => x.Period).Max();

            Report.PopulateReport(report, periodStart, periodEnd);
            model.Report = report;
            

            var html = RenderView("modals/definition", model);

            return Content(html);
        }

    }

}