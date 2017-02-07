using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace bank.web.models
{
    public class AppUser : IdentityUser
    {
        //add your custom properties which have not included in IdentityUser before
        //public string MyExtraProperty { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}