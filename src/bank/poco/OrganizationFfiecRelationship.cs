using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bank.poco
{
    public class OrganizationFfiecRelationship
    {
        public int? ParentOrganizationId { get; set; }
        public int? OffspringOrganizationId { get; set; }

        [XmlIgnore]
        public Organization ParentOrganization { get; set; }

        [XmlIgnore]
        public Organization OffspringOrganization { get; set; }

        public DateTime? DateRelationshipStart
        {
            get
            {
                return D_DT_START;
            }
        }

        public DateTime? DateRelationshipEnd
        {
            get
            {
                return D_DT_END.HasValue && D_DT_END.Value.Year < 9999 ? D_DT_END : null;
            }
        }

        public int? CTRL_IND { get; set; }

        public DateTime? D_DT_END { get; set; }

        public DateTime? D_DT_RELN_EST { get; set; }

        public DateTime? D_DT_START { get; set; }

        public int? DT_END { get; set; }

        public int? DT_RELN_EST { get; set; }

        public int? DT_START { get; set; }

        public int? EQUITY_IND { get; set; }

        public int? FC_IND { get; set; }

        public int? ID_RSSD_OFFSPRING { get; set; }

        public int? ID_RSSD_PARENT { get; set; }

        public int? MB_COST { get; set; }

        public int? OTHER_BASIS_IND { get; set; }

        public decimal? PCT_EQUITY { get; set; }

        public string PCT_EQUITY_BRACKET { get; set; }

        public string PCT_EQUITY_FORMAT { get; set; }

        public decimal? PCT_OTHER { get; set; }

        public int? REASON_ROW_CRTD { get; set; }

        public int? REASON_TERM_RELN { get; set; }

        public int? REG_IND { get; set; }

        public int? REGK_INV { get; set; }

        public int? RELN_LVL { get; set; }

    }

}
