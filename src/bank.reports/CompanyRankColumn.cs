using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports
{
    public class CompanyRankColumn : Column
    {
        public override ColumnTypes ColumnType
        {
            get
            {
                return ColumnTypes.CompanyRank;
            }
        }

        public override string HeaderText { get; set; }

        public override string Key
        {
            get
            {
                return "VariableCompany";
            }
        }

        public string OrderBy { get; set; }

        public override void SetFacts(IList<Fact> facts, IList<Concept> concepts)
        {
            var orderBy = OrderBy ?? concepts.First().Name.ToUpper();

            var list = facts.Where(x => x.Name == orderBy.ToUpper())
                .OrderByDescending(x => x.NumericValue)
                .Select(x => (CompanyFact)x)
                .Take(50)
                .ToList();

            var columns = new List<CompanyColumn>(50);
            var rank = 1;

            foreach (var fact in list)
            {
                var column = new CompanyColumn();
                var orgFacts = facts.Where(x => ((CompanyFact)x).OrganizationId == fact.OrganizationId).ToList();
                column.OrganizationId = fact.OrganizationId;
                column.SetFacts(orgFacts, concepts);
                column.Rank = rank++;
                columns.Add(column);
            }

            ChildColumns = columns;
        }
    }
}
