using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class BanksViewModel
    {
        public List<Organization> Banks { get; internal set; }
    }
}