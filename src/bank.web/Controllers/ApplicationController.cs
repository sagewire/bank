using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using bank.utilities;

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
    }

}