using System.Collections.Generic;
using bank.poco;
using bank.reports.charts;
using bank.reports.extensions;

namespace bank.reports
{
    public class ChartElement : TemplateElement
    {
        public ChartConfig ChartConfig { get; internal set; }
        //public List<Concept> Concepts { get; internal set; } = new List<Concept>();

        public override string Title
        {
            get
            {
                return ChartConfig.Title.ConceptReplace(Concepts);
            }
        }

        public string ChartDataType
        {
            get
            {
                return ChartConfig.ChartOverride ?? ChartConfig.ChartType.ToString().ToLower();
            }
        }

        public override void SetDataColumns(IList<Column> dataColumns)
        {
            base.SetDataColumns(dataColumns);
            ChartConfig.Columns = dataColumns;
        }

        public override IList<FactLookup> FactLookups
        {
            get
            {
                return ChartConfig.FactLookups;
            }
        }
    }

}