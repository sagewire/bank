using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public class TableRowGroup : ITableRow
    {
        public TableElement Table { get; internal set; }

        public TableRowTypes TableRowType
        {
            get
            {
                return TableRowTypes.Group;
            }
        }

        public string Label { get; set; }
        public bool Sum { get; set; }

        public string Format { get; set; } = "N0";
    }
}
