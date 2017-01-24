using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bank.charts
{
    public class ComboChartConfig : ChartConfig
    {
        public List<Series> Series { get; set; }

        protected override void Parse(XElement element)
        {
            base.Parse(element);
        }
    }
}
