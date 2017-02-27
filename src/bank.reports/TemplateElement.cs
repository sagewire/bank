using System.Collections.Generic;
using System.Linq;
using bank.poco;

namespace bank.reports
{
    public class TemplateElement
    {
        public virtual string Title { get; set; }
        public List<Concept> Concepts { get; set; } = new List<Concept>();
        public List<Concept> HiddenConcepts { get; set; } = new List<Concept>();

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


        private List<Concept> _allConcepts = null;
        public List<Concept> AllConcepts
        {
            get
            {
                if (_allConcepts == null)
                {
                    _allConcepts = GetConcepts(this.Concepts);

                    foreach(var item in HiddenConcepts)
                    {
                        var existing = _allConcepts.SingleOrDefault(x => x.Name == item.Name);

                        if (existing == null)
                        {
                            _allConcepts.Add(item);
                        }
                    }


                }
                return _allConcepts;
            }
        }

        private List<Concept> GetConcepts(List<Concept> concepts)
        {
            var allConcepts = new List<Concept>();
            allConcepts.AddRange(concepts);

            foreach (var concept in concepts)
            {
                allConcepts.AddRange(GetChildConcepts(concept));
            }

            return allConcepts;
        }

        public List<Concept> GetChildConcepts(Concept concept)
        {
            var concepts = new List<Concept>();

            if (concept.Children != null)
            {
                concepts.AddRange(concept.Children);

                foreach (var child in concept.Children)
                {
                    concepts.AddRange(GetChildConcepts(child));
                }
            }

            return concepts;
        }


        public virtual IList<FactLookup> FactLookups
        {
            get
            {
                var lookups = new List<FactLookup>();

                foreach (var concept in AllConcepts)
                {
                    var factLookup = new FactLookup
                    {
                        Columns = DataColumns,
                        ConceptKeys = Concept.GetConceptKeys(AllConcepts),
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
        public string SubText { get; internal set; }
    }
}