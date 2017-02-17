using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using bank.web.Controllers;
using bank.web.models;

namespace bank.web
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Database.SetInitializer<AuthDbContext>(null);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(HttpContext.Current.Request.RawUrl);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            try
            {
                Response.AddHeader("X-Developer", "mike prince");
            }
            catch { }
            
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            Response.StatusCode = 500;

            //if (httpException != null)
            //{
            //    var statusCode = httpException.GetHttpCode();
            //    switch (statusCode)
            //    {
            //        case 404:
            //            HandleError();
            //            Response.StatusCode = statusCode;
            //            break;
            //    }
            //}


            if (!Settings.FriendlyErrors) return;

            Response.Clear();
            Server.ClearError();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Errors";
            routeData.Values["action"] = "Error";
            routeData.Values["exception"] = exception;
            //Response.StatusCode = 500;


            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;
            IController errorController = new ErrorController();
            var wrapper = new HttpContextWrapper(Context);
            var rc = new RequestContext(wrapper, routeData);
            errorController.Execute(rc);
        }

    }
}
