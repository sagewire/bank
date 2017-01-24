using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.reports;
using bank.utilities;

namespace bank.poco
{
    public class SankeyChartConfig : ChartConfig
    {
        public List<ChartColumnConfig> Columns { get; set; } = new List<ChartColumnConfig>();
        public List<ChartRowConfig> Rows { get; set; } = new List<ChartRowConfig>();

        public override IList<Concept> Concepts
        {
            get
            {
                var results = new List<string>();
                var resolver = new FactResolver();
                foreach (var row in Rows)
                {
                    foreach (var value in row.Values)
                    {
                        var concepts = resolver.ParseConcepts(value.Text);
                        results.AddRange(concepts);
                    }
                }

                return results.Select(x => new Concept { MDRM = x }).ToList();
            }
        }

    }
}
