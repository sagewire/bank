using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class ColViewModel
    {
        public TemplateColumn Column { get; set; }
        public bool IsChild { get; set; } = false;

    }
}