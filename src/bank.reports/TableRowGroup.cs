using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public class TableRowGroup : TableElement, ITableRow
    {
        public TableRowTypes TableRowType { get; set; }

    }
}
