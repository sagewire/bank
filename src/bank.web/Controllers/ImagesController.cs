using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.poco;
using bank.web.models;

namespace bank.web.Controllers
{
    public class ImagesController : ApplicationController
    {


        public ActionResult Review(string id)
        {
            if (IsImageRequest(id))
            {
                var url = StripImageRequest(Request.Url.ToString());
                return Image(url, 1200, 600);
            }

            var reviewId = DecodeId(id);
            var model = new SingleReviewVewModel();
            model.Review = bank.data.sql.Reviews.GetReview(reviewId);
            model.Header.CustomClass = "facebook-image";

            return View("imagegen/review", model);
        }


        public ActionResult Lender(string id)
        {
            return Entity(id, EntityType.Lender);
        }

        public ActionResult LoanOfficer(string id)
        {
            return Entity(id, EntityType.LoanOfficer);
        }

        private ActionResult Entity(string id, EntityType entityType)
        {
            if (IsImageRequest(id))
            {
                var url = StripImageRequest(Request.Url.ToString());
                return Image(url, 1200, 600);
            }

            var decodedId = DecodeId(id);

            var model = new EntityImageViewModel();

            if (entityType == EntityType.Lender)
            {
                model.Entity = bank.data.sql.Profile.GetLender(decodedId);
            }
            else
            {
                model.Entity = bank.data.sql.Profile.GetLoanOfficer(decodedId);
            }

            model.Header.Entity = model.Entity;

            return View("imagegen/entity", model);
        }

        private bool IsImageRequest(string id)
        {
            return id.EndsWith(".png");
        }

        private string StripImageRequest(string id)
        {
            return id.Replace(".png", "");
        }

        public ActionResult Image(string url, int viewPortWidth, int viewPortHeight)
        {
            Response.CacheControl = HttpCacheability.Public.ToString();
            
            var phantom = new Process();
            phantom.StartInfo.FileName = Settings.PhantomJs;
            phantom.StartInfo.WorkingDirectory = Path.GetDirectoryName(Settings.PhantomJs);
            phantom.StartInfo.Arguments = string.Format("../../assets/scripts/rasterize.js {0} /dev/stdout {1} {2}", url, viewPortWidth, viewPortHeight);
            phantom.StartInfo.UseShellExecute = false;
            phantom.StartInfo.RedirectStandardOutput = true;
            phantom.StartInfo.RedirectStandardError = true;
            phantom.Start();

            var base64 = phantom.StandardOutput.ReadToEnd().Trim();

            phantom.WaitForExit(10000);



            var output = Convert.FromBase64String(base64);

            return File(output, "image/png");
        }

    }
}