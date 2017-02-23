using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports.charts
{
    public class Annotations
    {
        public List<Annotation> Items { get; set; }
        public string Name { get; set; }
        public bool Visible { get; internal set; }
    }
}
