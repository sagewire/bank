using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports
{
    [DebuggerDisplay("{MDRM} {Label}")]
    public class LineItem
    {
        public ChartConfig ChartConfig { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public List<Concept> Concepts { get; set; } = new List<Concept>();
        public MdrmDefinition Definition { get; set; }

        public string MDRM {
            get
            {
                if (Concepts != null && Concepts.Any(x=>!string.IsNullOrWhiteSpace(x.MDRM)))
                {
                    return Concepts.Single(x => !string.IsNullOrWhiteSpace(x.MDRM)).MDRM;
                }
                return null;
            }
        }

        private List<LineItem> _subSections;
        public List<LineItem> SubSections
        {
            get
            {
                if (_subSections == null)
                {
                    _subSections = LineItems.Where(x => x.Type == "subsection").ToList();

                    if (!_subSections.Any())
                    {
                        _subSections = new List<LineItem> { this };
                    }
                }
                return _subSections;
            }
        }

        public string Label { get; set; }
        public string LineNumber { get; set; }
        //public IDictionary<int, Fact> Facts { get; internal set; } = new Dictionary<int, Fact>();
        public string SubTemplate { get; set; }

        public string Type { get; set; }
    }

    public class Concept
    {
        public string MDRM { get; set; }
        public MdrmDefinition MdrmDefinition { get; internal set; }
    }
}
