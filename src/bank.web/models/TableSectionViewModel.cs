using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class TableSectionViewModel
    {
        public LineItem SubSection { get; set; }        
        public List<LineItem> LineItems { get; set; }
    }
}