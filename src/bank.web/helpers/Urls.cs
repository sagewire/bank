using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using bank.enums;
using bank.extensions;
using bank.poco;
using bank.utilities;

namespace bank.web.helpers
{
    public static class Urls
    {
        public static string Url(this UrlHelper urlHelper, Organization organization, DateTime? period = null, string template = null, string section = null, bool isCurrentPeriod = false, string companies = null)
        {
            var rv = new RouteValueDictionary();

            rv.Add("name", organization.Name.CreateSlug());
            rv.Add("id", Base26.Encode(organization.OrganizationId));
            rv.Add("template", template?.ToLower());
            rv.Add("section", section);
            
            if (period.HasValue && period < DateTime.Now.LastQuarterDate() && !isCurrentPeriod)
            {
                rv.Add("period", period.Value.ToString("yyyy-MM-dd"));
            }
            
            if (!string.IsNullOrWhiteSpace(companies))
            {
                rv.Add("c", companies);
            }

            return urlHelper.RouteUrl("report", rv);

        }

        public static string DefaultUrl(this UrlHelper urlHelper, string controller, string action = null)
        {

            var rv = new RouteValueDictionary();
            rv.Add("controller", controller);
            rv.Add("action", action);

            return urlHelper.RouteUrl("Default", rv);
        }

        //public static string Url(this UrlHelper urlHelper, Organization organization)
        //{
        //    var rv = new RouteValueDictionary();

        //    rv.Add("name", organization.Name.CreateSlug());
        //    rv.Add("id", Base26.Encode(organization.OrganizationId));
        //    rv.Add("template", null);

        //    return urlHelper.RouteUrl("report", rv);

        //}
    }
}