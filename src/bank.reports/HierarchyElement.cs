using System.Collections.Generic;
using bank.poco;
using bank.reports.charts;
using bank.reports.extensions;

namespace bank.reports
{
    public class HierarchyElement : TemplateElement
    {
        public string RelativeTo { get; internal set; }
    }

}