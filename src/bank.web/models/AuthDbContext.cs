using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace bank.web.models
{
    public class AuthDbContext : IdentityDbContext<AppUser>
    {
        public AuthDbContext() : base("bank")
        {

        }
        // Other part of codes still same 
        // You don't need to add AppUser and AppRole 
        // since automatically added by inheriting form IdentityDbContext<AppUser>

    }
}