using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.poco;
using bank.reports;
using bank.utilities;
using bank.web.models;
using Microsoft.AspNet.Identity;
using bank.web.helpers;

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

        private AppUser _appUser;
        public AppUser CurrentProfile
        {
            get
            {
                if (_appUser == null)
                {
                    _appUser = User.Profile();
                }
                return _appUser;
            }
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

        public void SetVisited(Organization org)
        {
            var visited = GetCookie("orgs");
            var value = string.Format("{0}|{1}", org.Name, org.ProfileUrl);

            if (visited != null)
            {
                var orgs = visited.Value.Split('~').ToList();

                if (!orgs.Contains(value))
                {
                    orgs.Insert(0, value);
                }
                value = string.Join("~", orgs);
            }

            SetCookie("orgs", value);
        }

        public HttpCookie GetCookie(string key)
        {
            return Request.Cookies.AllKeys.Contains(key) ? Request.Cookies[key] : null;
        }


        public void SetCookie(string key, string val)
        {
            //if (Request.Cookies.AllKeys.Contains(key))
            //    return;

            var cookie = new HttpCookie(key)
            {
                Value = val,
                Expires = DateTime.Now.AddYears(20)
            };

            Response.Cookies.Add(cookie);
        }

        protected void PopulateReportsAndColumns(Organization org, IList<int> companies, Layout layout, DateTime? period = null)
        {
            var periodStart = org.ReportImports.Select(x => x.Period).Min();
            var periodEnd = period.HasValue ? period.Value : org.ReportImports.Select(x => x.Period).Max();

            var orgRepo = new OrganizationRepository();

            var columns = GetColumns(org, companies);
            layout.DataColumns = columns;

            var tasks = new List<Task>();

            //var populateTask = Task.Run(() => Report.PopulateReports(model.Reports, columns, periodStart, periodEnd));
            var populateTask = Task.Run(() => layout.Populate(periodEnd));
            var orgTask = Task.Run(() => orgRepo.GetOrganizations(companies));

            tasks.Add(orgTask);
            tasks.Add(populateTask);


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
        }

        protected IList<Column> GetColumns(Organization org, IList<int> companies)
        {
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

            return columns;
        }

        //protected void PopulateReportsAndColumns(Organization org, IList<int> companies, IReports model, DateTime? period = null)
        //{
        //    var orgRepo = new OrganizationRepository();
        //    //var org = orgRepo.GetOrganization(orgId, true, true);

        //    var periodStart = org.ReportImports.Select(x => x.Period).Min();
        //    var periodEnd = period.HasValue ? period.Value : org.ReportImports.Select(x => x.Period).Max();

        //    var columns = new List<Column>();

        //    foreach (var companyId in companies)
        //    {
        //        columns.Add(new CompanyColumn
        //        {
        //            OrganizationId = companyId
        //        });
        //    }

        //    var statepg = Global.PeerGroups[org.StatePeerGroup];

        //    columns.Add(new PeerGroupColumn
        //    {
        //        PeerGroup = statepg.Code,
        //        HeaderText = string.Format("Peer Group {0}", statepg.Code)
        //    });

        //    foreach (var pg in org.CustomPeerGroups)
        //    {
        //        columns.Add(new PeerGroupCustomColumn
        //        {
        //            PeerGroupCustom = pg,
        //            HeaderText = string.Format("Peer Group {0}", pg.PeerGroupCode)
        //        });
        //    }

        //    foreach(var report in model.Reports)
        //    {
        //        report.Columns = columns;
        //    }

        //    var tasks = new List<Task>();

        //    var populateTask = Task.Run(() => Report.PopulateReports(model.Reports, columns, periodStart, periodEnd));
        //    var orgTask = Task.Run(() => orgRepo.GetOrganizations(companies));

        //    tasks.Add(orgTask);
        //    tasks.Add(populateTask);

        //    //foreach (var column in columns)
        //    //{
        //    //    var companyColumn = column as CompanyColumn;

        //    //    if (companyColumn != null)
        //    //    {
        //    //        if (companies != null)
        //    //        {
        //    //            companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
        //    //        }
        //    //    }
        //    //}


        //    Task.WaitAll(tasks.ToArray());


        //    foreach (var column in columns)
        //    {
        //        var companyColumn = column as CompanyColumn;

        //        if (companyColumn != null)
        //        {
        //            if (companies != null)
        //            {
        //                companyColumn.Organization = orgTask.Result.Single(x => x.OrganizationId == companyColumn.OrganizationId);
        //            }
        //        }
        //    }


        //    model.Organization = org;

        //}

        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

    }

}