using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;
using bank.poco;

namespace bank.reports.charts
{
    public class SankeyChartConfig : ChartConfig
    {
        public SankeyChartConfig()
        {
            ChartType = ChartTypes.Sankey;
        }
        
        public override IList<SeriesData> GetSeriesData()
        {
            var seriesData = new SankeySeriesData();
            ((SeriesData)seriesData).Chart = this;

            var column = Columns.First();

            seriesData.Data = new List<object>();

            var columns = new List<object>();

            foreach (var configColumn in this.SankeyColumns)
            {
                columns.Add(
                    new
                    {
                        type = configColumn.Type,
                        label = configColumn.Label
                    }
                    );
            }

            seriesData.Data.Add(columns);

            foreach(var configRow in this.SankeyRows)
            {
                var result = new List<object>();
                result.Add(configRow.Values[0]);
                result.Add(configRow.Values[1]);

                var facts = column.GetFacts(ConceptKeys);
                
                var fact = configRow.Concept.PrepareFact(facts);

                if (fact != null && fact.NumericValue.HasValue)
                {
                    result.Add(fact.NumericValue.Value);
                }
                else
                {
                    result.Add(0);
                }

                seriesData.Data.Add(result);

            }

            return new List<SeriesData> { seriesData };
        }

        private List<string> _conceptKeys;
        public IList<string> ConceptKeys
        {
            get
            {
                if (_conceptKeys == null)
                {
                    var conceptKeys = new List<string>();

                    foreach (var concept in Concepts)
                    {
                        conceptKeys.AddRange(concept.ConceptKeys);
                    }

                    _conceptKeys = conceptKeys;
                }
                return _conceptKeys;
            }
        }


        public IList<SankeyColumn> SankeyColumns { get; set; } = new List<SankeyColumn>();
        public IList<SankeyRow> SankeyRows { get; set; } = new List<SankeyRow>();

        public override IList<Column> VisibleColumns
        {
            get
            {
                return Columns;
            }
        }

        protected override void Parse(XElement element)
        {
            base.Parse(element);

            var configColumns = element.Elements("column");
            var configRows = element.Elements("row");

            foreach (var configColumn in configColumns)
            {
                var c = new SankeyColumn();
                c.Type = configColumn.SafeAttributeValue("type");
                c.Id = configColumn.SafeAttributeValue("id");
                c.Label = configColumn.SafeAttributeValue("label");

                SankeyColumns.Add(c);
            }

            foreach (var row in configRows)
            {
                var values = row.Elements("value");
                var counter = 0;
                var r = new SankeyRow();
                SankeyRows.Add(r);

                foreach (var value in values)
                {
                    r.Values.Add(value.Value);

                    if (counter++ == 2)
                    {
                        var concept = new Concept(value.Value);
                        r.Concept = concept;
                        base.Concepts.Add(concept);
                    }
                }
            }

        }

        public class SankeyColumn
        {
            public string Id { get; internal set; }
            public string Label { get; internal set; }
            public string Type { get; internal set; }
        }

        public class SankeyRow
        {
            public List<string> Values { get; internal set; } = new List<string>();
            public Concept Concept { get; set; }
        }
    }
}
