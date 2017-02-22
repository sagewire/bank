using System.Collections.Generic;
using System.Linq;
using bank.poco;

namespace bank.reports
{
    public class TemplateElement
    {
        public virtual string Title { get; set; }
        public List<Concept> Concepts { get; set; } = new List<Concept>();

        public IList<Column> DataColumns { get; set; }

        private List<Column> _visibleColumns;
        public virtual IList<Column> VisibleColumns
        {
            get
            {
                return _visibleColumns = DataColumns.Where(x => x.ShowColumn).ToList();
            }
        }
        public int? Lookback { get; set; }

        public virtual string Partial { get; set; }

        public virtual void SetDataColumns(IList<Column> dataColumns)
        {
            DataColumns = dataColumns;
        }


        public virtual IList<FactLookup> FactLookups
        {
            get
            {
                var lookups = new List<FactLookup>();

                foreach (var concept in Concepts)
                {
                    var factLookup = new FactLookup
                    {
                        Columns = DataColumns,
                        ConceptKeys = Concept.GetConceptKeys(Concepts),
                        Lookback = this.Lookback
                    };

                    lookups.Add(factLookup);
                }

                return lookups;
            }
        }

        public string DataSource { get; internal set; }

        public object Data { get; set; }
        public string CssClasses { get; internal set; }
    }
}