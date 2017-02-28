using System.Collections.Generic;
using bank.poco;
using bank.reports.charts;
using bank.reports.extensions;

namespace bank.reports
{
    public class HierarchyElement : TemplateElement
    {
        public int ExpandToLevel { get; internal set; }
        public string RelativeTo { get; internal set; }
    }

}