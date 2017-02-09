using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using bank.poco;

namespace bank.reports
{
    public abstract class Column
    {
        //public ConcurrentDictionary<string, Fact> Facts { get; protected set; } = new ConcurrentDictionary<string, Fact>();
        public ConcurrentDictionary<string, Fact> Facts { get; protected set; } = new ConcurrentDictionary<string, Fact>();

        public abstract string Key { get; }
        public abstract ColumnTypes ColumnType { get; }

        public abstract void SetFacts(IList<Fact> facts);

        public List<Fact> GetFacts(IList<string> keys)
        {
            var facts = Facts.Where(x => keys.Contains(x.Key));
            return facts.Select(x => x.Value).ToList();
        }

        public Fact GetCell(Concept concept, Column column)
        {
            if (column == null || concept == null)
            {
                return null;
            }

            var facts = column.GetFacts(concept.ConceptKeys);
            var fact = concept.PrepareFact(facts);

            return fact;

        }

        public abstract string HeaderText { get; set; }

        public bool ShowColumn
        {
            get
            {
                return Facts != null && Facts.Any();
            }
        }
    }
}