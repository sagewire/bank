using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class BankBasicsViewModel
    {
        public Organization Organization { get; set; }
        public bool LargeName { get; set; } = false;
        public bool ShowPhoneNumber { get; set; } = false;
        public bool ShowWebsite { get; set; } = false;
        public bool EnablePopover { get; set; } = true;
        public bool IncludeSchemaOrgProp { get; set; } = false;

        public string NameHeaderTag
        {
            get
            {
                return LargeName ? "h4" : "h6";
            }
        }
    }
}