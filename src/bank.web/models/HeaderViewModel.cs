using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class HeaderViewModel
    {
        public string HeaderImage { get; set; }
        public string HeaderTitle { get; set; }
        public string SubTitle { get; set; }

        public bool HasSubTitle
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SubTitle);
            }
        }
    }
}