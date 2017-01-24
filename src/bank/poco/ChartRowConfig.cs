using System.Collections.Generic;

namespace bank.poco
{
    public class ChartRowConfig
    {
        public List<ChartRowValueConfig> Values { get; set; } = new List<ChartRowValueConfig>();
    }
}