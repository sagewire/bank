using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace bank.web.helpers
{
    //public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    //{
    //    protected bool _canOverride = true;

    //    ...custom authorization code goes here.....

    //    public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
    //    {
    //        Don't authorize if the override attribute exists
    //        if (_canOverride && actionContext.ActionDescriptor.GetCustomAttributes<OverrideClaimsAuthorizeAttribute>().Any())
    //        {
    //            return;
    //        }
    //        base.OnAuthorization(actionContext);
    //    }

    //}

    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        public string MemberType { get; set; }
        //public string LocationID { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            ClaimsIdentity claimsIdentity;
            //var httpContext = HttpContext.Current;
            if (!(httpContext.User.Identity is ClaimsIdentity))
            {
                return false;
            }

            claimsIdentity = httpContext.User.Identity as ClaimsIdentity;
            var subIdClaims = claimsIdentity.FindFirst("MemberType");
            //var locIdClaims = claimsIdentity.FindFirst("LocationId");
            if (subIdClaims == null)// || locIdClaims == null)
            {
                // just extra defense
                return false;
            }

            var userSubId = subIdClaims.Value;
            var userLocId = subIdClaims.Value;

            // use your desired logic on 'userSubId' and `userLocId', maybe Contains if I get your example right?
            //if (!this.SubjectID.Contains(userSubId) || !this.LocationID.Contains(userLocId))
            //{
            //    return false;
            //}

            //Continue with the regular Authorize check
            //return base.IsAuthorized(actionContext);

            return base.AuthorizeCore(httpContext);
        }
        
    }

}