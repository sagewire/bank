using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace bank.web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                name: "modals",
                url: "modals/{controller}/{action}/{type}/{id}",
                defaults: new { controller = "", action = "", id = UrlParameter.Optional, type = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "privacy",
                url: "privacy",
                defaults: new { controller = "Pages", action = "Privacy" }
            );

            routes.MapRoute(
                name: "organization",
                url: "org/{name}.{id}",
                defaults: new { controller = "Profile", action = "Organization" }
            );

            //routes.MapRoute(
            //    name: "report",
            //    url: "bank/{name}.{id}/report/{period}/{template}",
            //    defaults: new { controller = "Reports", action = "Viewer" }
            //);

            routes.MapRoute(
                name: "report",
                url: "org/{name}.{id}/report/{template}/{section}/{period}",
                defaults: new { controller = "Report", action = "Viewer", section = UrlParameter.Optional, period = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "orgs",
                url: "org",
                defaults: new { controller = "Directory", action = "Index", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "fact",
                url: "fact/{id}/{text}/{companies}",
                defaults: new { controller = "Fact", action = "Details", id = UrlParameter.Optional, companies = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Pages", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
