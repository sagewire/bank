using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class ChartItemViewModel
    {
        public ChartItem ChartItem { get; set; }
        public Report Report { get; set; }

        public string ChartDataType
        {
            get
            {
                return ChartItem.ChartConfig.ChartOverride ?? ChartItem.ChartConfig.ChartType.ToString().ToLower();
            }
        }
    }
}