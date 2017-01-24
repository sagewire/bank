using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public class TableItem : LineItem
    {
        public override LineItemTypes LineItemType
        {
            get
            {
                return LineItemTypes.Table;
            }
        }
    }
}
