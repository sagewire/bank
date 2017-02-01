﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;

namespace bank.reports.charts
{
    public abstract class ChartConfig
    {
        public string Title { get; set; }
        public string CssClasses { get; set; }
        public ChartTypes ChartType { get; set; }
        public List<Concept> Concepts { get; set; } = new List<Concept>();
        public string ChartOverride { get; private set; }
        public IDictionary<string, string> Placeholders { get; set; }
        public int? Lookback { get; set; }

        private Guid _chartId = Guid.NewGuid();
        public Guid ChartId
        {
            get
            {
                return _chartId;
            }
        }

        public IList<Column> Columns { get; set; }
        public abstract IList<Column> VisibleColumns { get; }

        public abstract IList<SeriesData> GetSeriesData(Column column);
        public abstract IList<SeriesData> GetSeriesData(IList<Column> columns);

        public IList<FactLookup> FactLookups
        {
            get
            {
                var factLookup = new FactLookup
                {
                    Columns = VisibleColumns,
                    ConceptKeys = Concept.GetConceptKeys(Concepts),
                    Lookback = this.Lookback
                };

                if (factLookup.ConceptKeys.Any())
                {
                    return new FactLookup[] { factLookup };
                }
                else
                {
                    return new FactLookup[] { };
                }
            }
        }

        public static ChartConfig Build(XElement element, IDictionary<string, string> placeholders = null)
        {
            var chartTypeString = element.SafeAttributeValue("type");
            var chartType = (ChartTypes)Enum.Parse(typeof(ChartTypes), chartTypeString, true);

            ChartConfig chartConfig;

            switch (chartType)
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
            chartConfig.Placeholders = placeholders;
            chartConfig.Parse(element);

            return chartConfig;
        }

        public string Resolve(string text)
        {
            if (text != null && text.Contains("|"))
            {
                var pair = text.Split('|');
                var concept = Concepts.Single(x => x.Name == pair[0]);
                var result = "";

                switch (pair[1].ToLower())
                {
                    case "shortlabel":
                        result = concept.ShortLabel;
                        break;
                }

                return result;
            }
            else
            {
                return text;
            }
        }

        protected virtual void Parse(XElement element)
        {
            Title = element.SafeAttributeValue("title");
            CssClasses = element.SafeAttributeValue("css-classes");
            ChartOverride = element.SafeAttributeValue("chart-override");
            Lookback = element.SafeIntAttributeValue("lookback");
        }

    }
}