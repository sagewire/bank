using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bank.reports
{
    public class SubSection : LineItem
    {
        public override LineItemTypes LineItemType
        {
            get
            {
                return LineItemTypes.SubSection;
            }
        }
        
    }
}
