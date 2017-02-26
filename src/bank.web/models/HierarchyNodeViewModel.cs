using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public class HierarchyNodeViewModel
    {
        public Organization Organization { get; set; }
        public Concept Concept { get; set; }
        public Column Column { get; set; }

        public HierarchyElement Element { get; set; }
    }
}