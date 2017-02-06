using System;
using System.Collections.Generic;

namespace bank.reports
{
    public class TableElement : TemplateElement, ITableRow
    {
        public List<ITableRow> Rows { get; set; } = new List<ITableRow>();

        public TableRowTypes TableRowType
        {
            get
            {
                return TableRowTypes.Table;
            }
            
        }

        public object Orientation { get; internal set; }

        public TableElement()
        {
        }
    }
}