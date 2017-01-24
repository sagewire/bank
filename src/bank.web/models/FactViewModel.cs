using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using bank.poco;
using bank.extensions;

namespace bank.web.models
{
    public class FactViewModel
    {
        public IList<int> CompanyIds { get; set; }
        public List<Fact> Facts { get; internal set; }
        public int RootCompanyId { get; internal set; }



        //public MvcHtmlString ValueAxes
        //{
        //    get
        //    {
        //        var axes = new List<object>();

        //        foreach (var company in CompanyIds)
        //        {
        //            var axis = new
        //            {
        //                id = "c" + company.ToString(),
        //                valueField = "c" + company.ToString()
        //            };

        //            axes.Add(axis);
        //        }

        //        return new MvcHtmlString(axes.ToJson());
        //    }
        //}


        public MvcHtmlString Series
        {
            get
            {
                var series = new List<object>();

                foreach (var company in CompanyIds)
                {

                    var data = Facts.Where(x => x.OrganizationId == company)
                                    .Select(x => new { x = x.Period.Value.ToMillisecondsSince1970(), y = x.NumericValue })
                                    .ToArray();

                    var seriesItem = new
                    {
                        name = company.ToString(),
                        data = data,
                        showInNavigator = true
                    };

                    series.Add(seriesItem);
                }

                return new MvcHtmlString(series.ToJson());
            }
        }


        //public MvcHtmlString Graphs
        //{
        //    get
        //    {
        //        var axes = new List<object>();

        //        foreach (var company in CompanyIds)
        //        {
        //            var axis = new
        //            {
        //                id = "g" + company.ToString(),
        //                valueField = "c" + company.ToString(),
        //                title = company.ToString()
        //            };

        //            axes.Add(axis);
        //        }

        //        return new MvcHtmlString(axes.ToJson());
        //    }
        //}

        //public MvcHtmlString DataProvider
        //{
        //    get
        //    {
        //        var dataProviders = new List<object>();

        //        foreach (var company in CompanyIds)
        //        {
        //            var facts = Facts.Where(x => x.OrganizationId == company);
        //            foreach(var fact in facts)
        //            {
        //                var datum = new Dictionary<string, object>();
        //                datum.Add("c" + company, (int)fact.NumericValue);
        //                datum.Add("date", fact.Period.Value.ToString("yyyy-MM-dd"));
        //                dataProviders.Add(datum);
        //            }
                    
        //        }

        //        return new MvcHtmlString(dataProviders.ToJson());
        //    }
        //}
    }
}