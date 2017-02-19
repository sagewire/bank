using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.poco.graph
{
    public interface IEdge
    {
        long SourceId { get; set; }
        long TargetId { get; set; }

        string Key { get; }
        string Label { get; set; }
    }
}
