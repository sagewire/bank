using System.Collections.Generic;
using bank.poco;

namespace bank.reports
{
    public class TemplateElement
    {
        public virtual string Title { get; set; }
        public List<Concept> Concepts { get; set; } = new List<Concept>();

        public IList<Column> DataColumns { get; set; }

        public virtual IList<Column> VisibleColumns
        {
            get
            {
                return DataColumns;
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
                        Columns = VisibleColumns,
                        ConceptKeys = Concept.GetConceptKeys(Concepts),
                        Lookback = this.Lookback
                    };

                    lookups.Add(factLookup);
                }

                return lookups;
            }
        }
    }
}