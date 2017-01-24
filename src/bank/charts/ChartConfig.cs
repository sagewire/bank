using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;

namespace bank.charts
{
    public abstract class ChartConfig
    {
        public string Title { get; set; }
        public ChartTypes ChartType { get; set; }

        public static ChartConfig Build(XElement element)
        {
            var chartTypeString = element.SafeAttributeValue("type");
            var chartType = (ChartTypes)Enum.Parse(typeof(ChartTypes), chartTypeString, true);

            ChartConfig chartConfig;

            switch(chartType)
            {
                case ChartTypes.Combo:
                    chartConfig = new ComboChartConfig();
                    break;
                case ChartTypes.Sankey:
                    chartConfig = new SankeyChartConfig();
                    break;
                default:
                    throw new Exception("Chart type not supported");
            }

            chartConfig.Parse(element);

            return chartConfig;
        }
        
        protected virtual void Parse(XElement element)
        {
            Title = element.SafeAttributeValue("title");
        }

    }
}
