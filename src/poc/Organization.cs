using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable()]
[System.Xml.Serialization.XmlRoot("DATA")]
public class OrganizationCollection
{
    //[XmlArray("DATA")]
    //[XmlArrayItem("ATTRIBUTES", typeof(Organization))]
    [XmlElement("ATTRIBUTES")]
    public Organization[] Organization { get; set; }
}

[Serializable()]
public class Organization
{
    public string ACT_PRIM_CD { get; set; }

    public int? AUTH_REG_DIST_FRS { get; set; }

    public int? BANK_CNT { get; set; }

    public int? BHC_IND { get; set; }

    public int? BNK_TYPE_ANALYS_CD { get; set; }

    public int? BROAD_REG_CD { get; set; }

    public int? CHTR_AUTH_CD { get; set; }

    public int? CHTR_TYPE_CD { get; set; }

    public string CITY { get; set; }

    public int? CNSRVTR_CD { get; set; }

    public int? CNTRY_CD { get; set; }

    public int? CNTRY_INC_CD { get; set; }

    public string CNTRY_INC_NM { get; set; }

    public string CNTRY_NM { get; set; }

    public int? COUNTY_CD { get; set; }

    public DateTime? D_DT_END { get; set; }

    public DateTime? D_DT_EXIST_CMNC { get; set; }

    public DateTime? D_DT_EXIST_TERM { get; set; }

    public DateTime? D_DT_INSUR { get; set; }

    public DateTime? D_DT_OPEN { get; set; }

    public DateTime? D_DT_START { get; set; }

    public int? DIST_FRS { get; set; }

    public string DOMESTIC_IND { get; set; }

    public int? DT_END { get; set; }

    public int? DT_EXIST_CMNC { get; set; }

    public int? DT_EXIST_TERM { get; set; }

    public int? DT_INSUR { get; set; }

    public int? DT_OPEN { get; set; }

    public int? DT_START { get; set; }

    public string ENTITY_TYPE { get; set; }

    public int? EST_TYPE_CD { get; set; }

    public int? FBO_4C9_IND { get; set; }

    public int? FHC_IND { get; set; }

    public int? FISC_YREND_MMDD { get; set; }

    public int? FNCL_SUB_HOLDER { get; set; }

    public int? FNCL_SUB_IND { get; set; }

    public int? FUNC_REG { get; set; }

    public int? IBA_GRNDFTHR_IND { get; set; }

    public int? IBF_IND { get; set; }

    public int? ID_ABA_PRIM { get; set; }

    public string ID_CUSIP { get; set; }

    public int? ID_FDIC_CERT { get; set; }

    public string ID_LEI { get; set; }

    public int? ID_NCUA { get; set; }

    public int? ID_OCC { get; set; }

    public int? ID_RSSD { get; set; }

    public int? ID_RSSD_HD_OFF { get; set; }

    public int? ID_TAX { get; set; }

    public int? ID_THRIFT { get; set; }

    public string ID_THRIFT_HC { get; set; }

    public int? INSUR_PRI_CD { get; set; }

    public int? MBR_FHLBS_IND { get; set; }

    public int? MBR_FRS_IND { get; set; }

    public int? MJR_OWN_MNRTY { get; set; }

    public string NM_LGL { get; set; }

    public string NM_SHORT { get; set; }

    public int? NM_SRCH_CD { get; set; }

    public int? ORG_TYPE_CD { get; set; }

    public int? PLACE_CD { get; set; }

    public string PRIM_FED_REG { get; set; }

    public string PROV_REGION { get; set; }

    public int? REASON_TERM_CD { get; set; }

    public int? SEC_RPTG_STATUS { get; set; }

    public int? SLHC_IND { get; set; }

    public int? SLHC_TYPE_IND { get; set; }

    public string STATE_ABBR_NM { get; set; }

    public int? STATE_CD { get; set; }

    public int? STATE_HOME_CD { get; set; }

    public string STATE_INC_ABBR_NM { get; set; }

    public int? STATE_INC_CD { get; set; }

    public string STREET_LINE1 { get; set; }

    public string STREET_LINE2 { get; set; }

    public string URL { get; set; }

    public string ZIP_CD { get; set; }

}
