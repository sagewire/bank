using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace bank.web.helpers
{
    public static class StringExtensions
    {
        public static MvcHtmlString ToMvcHtmlString(this string text)
        {
            return new MvcHtmlString(text);
        }
    }
}