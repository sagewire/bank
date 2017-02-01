using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports
{
    public class PeerGroupColumn : Column
    {

        public string PeerGroup { get; set; }

        private ColumnTypes _columnType = ColumnTypes.PeerGroup;
        public override ColumnTypes ColumnType
        {
            get
            {
                return _columnType;
            }
        }

        public override string Key
        {
            get
            {
                return string.Format("{0}|{1}", ColumnType, PeerGroup);
            }
        }

        private string _headerText;
        public override string HeaderText
        {
            get
            {
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        public override void SetFacts(IList<Fact> facts)
        {
            //var existing = Facts.Select(x => x..Name).Distinct().ToList();
            var filtered = facts.Where(x => x.FactType == enums.FactTypes.PeerGroup && ((PeerGroupFact)x).PeerGroup == PeerGroup);

            var distinct = filtered.Distinct().ToList();

            foreach (var fact in distinct)
            {
                Facts.TryAdd(fact.Name, fact);
                //Facts.AddOrUpdate(fact.Name, fact, (key, oldValue) => fact);
            }
        }

    }
}
