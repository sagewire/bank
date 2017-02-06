using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public interface ITableRow
    {
        TableRowTypes TableRowType { get;  }
    }
    
    public enum TableRowTypes
    {
        Concept,
        Header,
        Group,
        Table
    }
}
