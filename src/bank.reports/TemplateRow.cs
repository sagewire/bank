using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports
{
    public class TemplateRow
    {
        public IList<TemplateColumn> Columns { get; set; } = new List<TemplateColumn>();

    }
}
