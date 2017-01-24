using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bank.reports.charts
{
    public class SankeyChartConfig : ChartConfig
    {
        public override IList<SeriesData> GetSeriesData(Column column)
        {
            throw new NotImplementedException();
        }

        protected override void Parse(XElement element)
        {
            throw new NotImplementedException();
        }
    }
}
