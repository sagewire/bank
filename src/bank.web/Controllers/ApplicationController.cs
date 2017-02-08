using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using bank.data.repositories;
using bank.poco;
using bank.reports;
using bank.utilities;
using bank.web.models;
using Microsoft.AspNet.Identity;
using bank.web.helpers;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace bank.web.Controllers
{
    public class ApplicationController : Controller
    {
        private IAuthenticationManager _AuthenticationManager;
        private AppUserManager _UserManager;

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
            if (ids == null) return new int[0];
            return DecodeIds(ids.Split(','));
        }

        public IList<int> DecodeIds(IList<string> ids)
        {
            var intList = new List<int>();

            if (ids == null)
            {
                return intList;
            }

            foreach (var s in ids.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var decodedId = DecodeId(s);
                intList.Add(decodedId);
            }

            return intList.ToArray();
        }

        protected IAuthenticationManager AuthenticationManager
        {
            get
            {
                return _AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            }
        }

        public AppUserManager UserManager
        {
            get
            {
                return _UserManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppUser _appUser;
        public AppUser CurrentProfile
        {
            get
            {
                if (Request.IsAuthenticated && _appUser == null)
                {
                    
                    _appUser = UserManager.FindByName(User.Identity.Name);
                    
                    var favRepo = new UserFavoriteRepository();
                    _appUser.Favorites = favRepo.GetFavorites(_appUser.Id, FavoriteTypes.User);
                    _appUser.RecentVisits = favRepo.GetFavorites(_appUser.Id, FavoriteTypes.Visit);
                }
                return _appUser;
            }
        }

        public void SetProfileVisit(int organizationId)
        {
            Task.Run(() =>
            {
                try {
                    var userId = User.Identity.GetUserId();

                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return;
                    }

                    var visit = new UserFavorite
                    {
                        OrganizationId = organizationId,
                        UserId = userId,
                        FavoriteType = FavoriteTypes.Visit,
                        LastVisited = DateTime.Now
                    };

                    var repo = new UserFavoriteRepository();
                    repo.Save(visit);
                }
                catch(Exception ex)
                {
                    throw;
                }
            });
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

        public void SetVisited(Organization org)
        {
            var visited = GetCookie("orgs");
            var value = string.Format("{0}|{1}", org.Name, org.ProfileUrl);

            if (visited != null)
            {
                var orgs = visited.Value.Split('~').ToList();

                if (!orgs.Contains(value))
                {
                    orgs.Insert(0, value);
                }
                value = string.Join("~", orgs);
            }

            SetCookie("orgs", value);
        }

        public HttpCookie GetCookie(string key)
        {
            return Request.Cookies.AllKeys.Contains(key) ? Request.Cookies[key] : null;
        }


        public void SetCookie(string key, string val)
        {
            //if (Request.Cookies.AllKeys.Contains(key))
            //    return;

            var cookie = new HttpCookie(key)
            {
                Value = val,
                Expires = DateTime.Now.AddYears(20)
            };

            Response.Cookies.Add(cookie);
        }
        
        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

    }

}