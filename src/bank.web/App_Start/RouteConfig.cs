﻿using System;
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

            //routes.MapRoute(
            //    name: "modals",
            //    url: "modals/{controller}/{action}/{type}/{id}",
            //    defaults: new { controller = "", action = "", id = UrlParameter.Optional, type = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "data",
                url: "data/{controller}/{action}/{type}/{id}",
                defaults: new { controller = "", action = "Default", id = UrlParameter.Optional, type = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "privacy",
                url: "privacy",
                defaults: new { controller = "Pages", action = "Privacy" }
            );

            routes.MapRoute(
                name: "home",
                url: "",
                defaults: new { controller = "Pages", action = "Placeholder", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "report",
                url: "org/{name}.{id}/{template}/{section}/{period}",
                defaults: new { controller = "Profile", action = "Viewer", template = UrlParameter.Optional, section = UrlParameter.Optional, period = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "fact",
                url: "fact/{id}/{text}/{companies}",
                defaults: new { controller = "Fact", action = "Details", id = UrlParameter.Optional, companies = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "dashboard",
                url: "dashboard/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );

                        routes.MapRoute(
                name: "account",
                url: "account/{action}/{id}",
                defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "report-gen",
                url: "{*segment}",
                defaults: new { controller = "Report", action = "Index", segment = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Pages", action = "Placeholder", id = UrlParameter.Optional }
            );

        }
    }
}
