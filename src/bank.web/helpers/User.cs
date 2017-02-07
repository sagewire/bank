using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.web.models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Principal;

namespace bank.web.helpers
{
    public static class User
    {
        public static AppUser Profile(this IPrincipal user)
        {
            var usermanager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();

            var appUser = usermanager.FindByName(user.Identity.Name);

            return appUser;
        }
    }
}