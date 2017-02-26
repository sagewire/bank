using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.reports.charts;

namespace bank.reports
{
    public class TemplateColumn
    {
        public string CssClasses { get; set; }
        public IList<TemplateRow> Rows { get; set; }

        public IList<ChartConfig> Charts { get; set; }
        public IList<TemplateElement> Elements { get; set; }
        public string GridOverride { get; internal set; }
        public bool UseContainer { get; internal set; }

        //public bool IsChild { get; set; } = false;
    }
}
