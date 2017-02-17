using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using bank.extensions;

namespace bank.poco
{
    [Serializable()]
    public class OrganizationFfiecTransformation
    {
        [XmlIgnore()]
        public Organization PredecessorOrganization { get; set; }
        [XmlIgnore()]
        public Organization SuccessorOrganization { get; internal set; }
        public int? PredecessorOrganizationId { get; set; }
        public int? SuccessorOrganizationId { get; set; }
        public int? ACCT_METHOD { get; set; }

        public DateTime? D_DT_TRANS { get; set; }

        public int? DT_TRANS { get; set; }

        public int? ID_RSSD_PREDECESSOR { get; set; }

        public int? ID_RSSD_SUCCESSOR { get; set; }

        public int? TRNSFM_CD { get; set; }

        public DateTime RoundedToQuarter
        {
            get
            {
                var same = D_DT_TRANS.Value == D_DT_TRANS.Value.LastQuarterDate();

                if (same)
                {
                    return D_DT_TRANS.Value;
                }
                else
                {
                    return D_DT_TRANS.Value.LastQuarterDate().AddQuarters(1);
                }

            }
        }
        
    }

}
