using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class TableElementViewModel
    {
        public TableElement Table { get; set; }
        public bool IsGroup { get; set; } = false;
    }
}