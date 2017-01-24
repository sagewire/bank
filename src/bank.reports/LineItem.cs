using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    [DebuggerDisplay("{Id} - {Label}")]
    public abstract class LineItem : ICommonAttributes
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public List<Concept> Concepts { get; set; } = new List<Concept>();

        public abstract LineItemTypes LineItemType { get; }

        public bool IsChart
        {
            get
            {
                return LineItemType == LineItemTypes.Chart || LineItemType == LineItemTypes.Charts;
            }
        }
    }
}
