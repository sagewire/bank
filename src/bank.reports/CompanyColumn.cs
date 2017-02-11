using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports
{
    public class CompanyColumn : Column
    {
        public int? Rank { get; set; }
        public override ColumnTypes ColumnType
        {
            get
            {
                return ColumnTypes.Company;
            }
        }

        public override string HeaderText
        {
            get
            {
                return Organization.Name;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override string Key
        {
            get
            {
                string format = "{0}|{1}";
                return string.Format(format, ColumnType.ToString(), OrganizationId);
            }
        }

        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }
        
        public override void SetFacts(IList<Fact> facts, IList<Concept> concepts)
        {
            //var existing = Facts.Select(x => x..Name).Distinct().ToList();
            var filtered = facts.Where(x => x.FactType == enums.FactTypes.Company && ((CompanyFact)x).OrganizationId == OrganizationId);
            
            var distinct = filtered.Distinct().ToList();

            foreach(var fact in distinct)
            {
                Facts.TryAdd(fact.Name, fact);
                //Facts.AddOrUpdate(fact.Name, fact, (key, oldValue) => fact);
            }
        }
        
    }
}
