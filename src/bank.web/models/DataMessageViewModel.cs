using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bank.web.models
{
    public class DataMessageViewModel
    {
        public string Header { get; set; }
        public string Message { get; set; }

        public string Icon { get; set; }
        public string IconClass { get; set; }

        public bool HasIcon
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Icon);
            }
        }
    }
}