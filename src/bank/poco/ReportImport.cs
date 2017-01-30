using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.enums;

namespace bank.poco
{
    public class ReportImport
    {
        public int OrganizationId { get; set; }
        public ReportTypes ReportType { get; set; }
        public DateTime Quarter { get; set; }
    }
}
