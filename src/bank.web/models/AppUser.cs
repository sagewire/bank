using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using bank.poco;
using Microsoft.AspNet.Identity.EntityFramework;

namespace bank.web.models
{
    public class AppUser : IdentityUser
    {

        [NotMapped]
        public IList<Organization> Favorites { get; internal set; }

        [NotMapped]
        public IList<Organization> RecentVisits { get; internal set; }


        //add your custom properties which have not included in IdentityUser before
        //public string MyExtraProperty { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}