using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.reports;
using bank.reports.formulas;
using bank.utilities;
using bank.web.models;

namespace bank.web.Controllers
{
    public class ApplicationController : Controller
    {

        public int DecodeId(string id)
        {
            int incomingId;
            if (!int.TryParse(id, out incomingId))
            {
                incomingId = (int)Base26.Decode(id);
            }

            return incomingId;
        }

        public IList<int> DecodeIds(string ids)
        {
            var intList = new List<int>();

            if (string.IsNullOrWhiteSpace(ids))
            {
                return intList;
            }

            var list = ids.Split(',');

            foreach(var s in list.Where(x=>!string.IsNullOrWhiteSpace(x)))
            {
                var decodedId = DecodeId(s);
                intList.Add(decodedId);
            }

            return intList.ToArray();
        }

        public string RenderView(string viewName, object model)
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

        protected void PopulateReportsAndColumns(int orgId, IList<int> companies, IReports model)
        {
            var orgRepo = new OrganizationRepository();
            var org = orgRepo.GetOrganization(orgId, true, true);

            var periodStart = org.ReportImports.Select(x => x.Quarter).Min();
            var periodEnd = org.ReportImports.Select(x => x.Quarter).Max();
                
            var columns = new List<Column>();

            foreach (var companyId in companies)
            {
                columns.Add(new CompanyColumn
                {
                    OrganizationId = companyId
                });
            }

            var statepg = Global.PeerGroups[org.StatePeerGroup];

            columns.Add(new PeerGroupColumn
            {
                PeerGroup = statepg.Code,
                HeaderText = string.Format("Peer Group {0}", statepg.Code)
            });

            foreach (var pg in org.CustomPeerGroups)
            {
                columns.Add(new PeerGroupCustomColumn
                {
                    PeerGroupCustom = pg,
                    HeaderText = string.Format("Peer Group {0}", pg.PeerGroupCode)
                });
            }

            foreach(var report in model.Reports)
            {
                report.Columns = columns;
            }

            var tasks = new List<Task>();
            
            var populateTask = Task.Run(() => Report.PopulateReports(model.Reports, columns, periodStart, periodEnd));
            var orgTask = Task.Run(() => orgRepo.GetOrganizations(companies));

            tasks.Add(orgTask);
            tasks.Add(populateTask);

            foreach (var column in columns)
            {
                var companyColumn = column as CompanyColumn;

                if (companyColumn != null)
                {
                    if (companies != null)
                    {
                        companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                    }
                }
            }


            Task.WaitAll(tasks.ToArray());


            foreach (var column in columns)
            {
                var companyColumn = column as CompanyColumn;

                if (companyColumn != null)
                {
                    if (companies != null)
                    {
                        companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
                    }
                }
            }


            model.Organization = org;

        }
    }

}