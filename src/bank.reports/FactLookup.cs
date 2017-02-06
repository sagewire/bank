using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public class FactLookup
    {
        //public IList<int> OrganizationIds { get; set; } = new List<int>();
        public IList<string> ConceptKeys = new List<string>();
        public int? Lookback { get; set; }
        public IList<Column> Columns { get; set; }


        public IList<int> OrganizationIds
        {
            get
            {
                return Columns.Where(x => x.ColumnType == ColumnTypes.Company).Select(x => ((CompanyColumn)x).OrganizationId).Distinct().ToList();
            }
        }

        public IList<string> PeerGroups
        {
            get
            {
                return Columns.Where(x => x.ColumnType == ColumnTypes.PeerGroup).Select(x => ((PeerGroupColumn)x).PeerGroup).Distinct().ToList();
            }
        }

        public string Key
        {
            get
            {
                string format = "{0}-{1}";

                return string.Format(format, Lookback, string.Join(",", Columns.Select(x => x.Key).OrderBy(x => x)));
            }
        }

        public static List<FactLookup> Merge(IList<FactLookup> sourceList, IList<FactLookup> targetList)
        {

            if (sourceList == null || !sourceList.Any()) return targetList.ToList();

            var lookup = targetList.ToDictionary(x => x.Key, x => x);

            foreach (var source in sourceList)
            {
                
                if (lookup.ContainsKey(source.Key))
                {
                    var target = lookup[source.Key];

                    foreach (var conceptKey in source.ConceptKeys)
                    {
                        target.ConceptKeys.Add(conceptKey);
                    }
                }
                else
                {
                    lookup.Add(source.Key, source);
                }

            }

            return lookup.Values.ToList();

        }
    }
}
