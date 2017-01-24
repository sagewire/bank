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
        public static string Url(this UrlHelper urlHelper, Organization organization, DateTime period, string template, string section, bool isCurrentPeriod = false)
        {
            var rv = new RouteValueDictionary();

            rv.Add("name", organization.Name.CreateSlug());
            rv.Add("id", Base26.Encode(organization.OrganizationId));
            rv.Add("template", template.ToLower());
            rv.Add("section", section);

            if (period < DateTime.Now.LastQuarterDate() && !isCurrentPeriod)
            {
                rv.Add("period", period.ToString("yyyy-MM-dd"));
            }
            
            return urlHelper.RouteUrl("report", rv);

        }

        public static string Url(this UrlHelper urlHelper, Organization organization)
        {
            var rv = new RouteValueDictionary();

            rv.Add("name", organization.Name.CreateSlug());
            rv.Add("id", Base26.Encode(organization.OrganizationId));

            return urlHelper.RouteUrl("bank", rv);

        }
    }
}