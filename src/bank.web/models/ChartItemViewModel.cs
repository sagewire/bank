using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;
using bank.reports.charts;

namespace bank.web.models
{
    public class ChartItemViewModel
    {
        public ChartConfig ChartConfig { get; set; }
        public Report Report { get; set; }
        public bool ShowTitle { get; set; }
        public string ChartDataType
        {
            get
            {
                return ChartConfig.ChartOverride ?? ChartConfig.ChartType.ToString().ToLower();
            }
        }
    }
}