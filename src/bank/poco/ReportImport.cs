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
        public DateTime Period { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Filed { get; set; }
        public DateTime? Processed { get; set; }
        public string State { get; set; }

        public string ReportTypeAsString
        {
            get
            {
                return ReportType.ToString();
            }
        }
    }
}
