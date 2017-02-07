using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.poco;
using bank.reports;
using bank.web.models;
using bank.extensions;
using bank.web.helpers;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace bank.web.Controllers
{
    public class FavoritesController : ApplicationController
    {
        [HttpGet]
        [Authorize403]
        [ActionName("Default")]
        public async Task<ActionResult> Get(string id)
        {

            var repo = new UserFavoriteRepository();
            var fav = new UserFavorite
            {
                OrganizationId = int.Parse(id),
                UserId = User.Identity.GetUserId(),
                FavoriteType = FavoriteTypes.User
            };


            repo.Save(fav);

            var model = new DataMessageViewModel
            {
                Header = "Favorite added to your dashboard"
            };

            var controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            var result = new DataResponse
            {
                Success = true,
                Html = RenderView("_DataMessage", model)
            };

            var json = JsonConvert.SerializeObject(result);

            //return View(string.Format("modals/{0}", controllerName.ToLower()), model);

            return Content(json);
        }

    }

}
