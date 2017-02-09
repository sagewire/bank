using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class TableRowViewModel
    {
        public TableRow Row { get; set; }
        public IList<Column> Columns { get; set; }
        public TableElement Table { get; set; }

        public string Level
        {
            get
            {
                return string.Format("level-{0}", Table.Level);
            }
        }
    }
}