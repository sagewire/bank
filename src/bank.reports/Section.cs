using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace bank.reports
{
    public class Section : LineItem
    {
        public override LineItemTypes LineItemType
        {
            get
            {
                return LineItemTypes.Section;
            }
        }

        private List<SubSection> _subSections;
        public List<SubSection> SubSections
        {
            get
            {
                return _subSections;
            }
            set
            {
                _subSections = value;
            }
        }
    }
}
