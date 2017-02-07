using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using bank.poco;
using bank.web.models;
using bank.extensions;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Principal;
using System.Web.Routing;

namespace bank.web.Controllers
{
    public class AccountController : ApplicationController
    {
        private IAuthenticationManager AuthenticationManager;
        private AppUserManager UserManager;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            UserManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            
            base.OnActionExecuting(filterContext);
        }

        [HttpPost]
        public ActionResult Signin(LoginViewModel login)
        {
            if (ModelState.IsValid)
            {
                //var userManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
                //var authManager = HttpContext.GetOwinContext().Authentication;

                AppUser user = UserManager.Find(login.Email, login.Password);
                if (user != null)
                {
                    var ident = UserManager.CreateIdentity(user,
                        DefaultAuthenticationTypes.ApplicationCookie);

                    AuthenticationManager.SignIn(
                        new AuthenticationProperties { IsPersistent = false }, ident);


                    return Redirect(login.ReturnUrl ?? Url.RouteUrl("dashboard", new { action = "Index" } ));
                }
            }
            ModelState.AddModelError("", "Invalid username or password");
            return View(login);
        }

        [HttpGet]
        public ActionResult Signin()
        {
            return View();
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var vm = new RegisterViewModel();
            //vm.IsModal = CheckModal();
            //vm.ReturnUrl = ReturnUrl();

            return View(vm);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl)
        {
            //model.IsModal = CheckModal();
            //model.ReturnUrl = ReturnUrl();
            ModelState.Clear();
            model.Password = "letmein123";
            model.ConfirmPassword = "letmein123";

            
            if (TryValidateModel(model))
            {
                //var userManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
                //var authManager = HttpContext.GetOwinContext().Authentication;

                var AppMember = new AppUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(AppMember, model.Password);
                
                if (result.Succeeded)
                {
                    AppUser user = UserManager.Find(model.Email, model.Password);
                    UserManager.AddClaim(user.Id, new System.Security.Claims.Claim("MemberType", "free"));

                    var ident = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, ident);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(AppMember.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = AppMember.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(AppMember.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToLocal(Url.RouteUrl("dashboard", new { action = "Index" }));
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/";
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                //string url = string.Format("{0}?{1}", returnUrl, Guid.NewGuid());
                string url = returnUrl;
                return Redirect(url);
            }
            return RedirectToAction("Pages", "Index");
        }

        [AllowAnonymous]
        public ActionResult SignOut(string returnUrl)
        {
            AuthenticationManager.SignOut();//8777 DefaultAuthenticationTypes.ApplicationCookie);
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            return View();


        }

    }
}