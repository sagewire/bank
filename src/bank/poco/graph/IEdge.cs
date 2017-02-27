using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.poco.graph
{
    public interface IEdge
    {
        object SourceId { get; set; }
        object TargetId { get; set; }

        string Key { get; }
        string Label { get; set; }
    }
}
