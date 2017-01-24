using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.reports
{
    public class CompanyFactColumn : FactColumn
    {
        public CompanyFactColumn() : base(FactColumnType.Company)
        {

        }
        public Organization Organization { get; set; }

        public override string Header
        {
            get
            {
                return Organization.Name;
            }
        }

    }
}
