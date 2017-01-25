using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;
using bank.reports.charts;

namespace bank.web.models
{
    public class ConceptDefinitionViewModel
    {
        public ConceptDefinition Definition { get; internal set; }
        public string Mdrm { get; set; }
        public Report Report { get; set; }

        public ComboChartConfig Chart { get; set; }

    }
}