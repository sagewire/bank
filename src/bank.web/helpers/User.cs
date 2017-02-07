using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.web.models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Principal;
using bank.data.repositories;

namespace bank.web.helpers
{
    //public static class User
    //{
    //    public static AppUser Profile(this IPrincipal user, bool loadFavorites = false)
    //    {
    //        var usermanager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();

    //        var appUser = usermanager.FindByName(user.Identity.Name);

    //        if (loadFavorites)
    //        {
    //            var favRepo = new UserFavoriteRepository();
    //            appUser.Favorites = favRepo.GetFavorites(appUser.Id);
    //        }

    //        return appUser;
    //    }
    //}
}