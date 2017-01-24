using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.reports.charts;

namespace bank.reports
{
    public class ChartItem : LineItem
    {
        public ChartConfig ChartConfig { get; set; }

        public override LineItemTypes LineItemType
        {
            get
            {
                return LineItemTypes.Chart;
            }
        }
    }
}
