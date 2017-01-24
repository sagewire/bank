using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace bank.enums
{
    [DataContract]
    public enum ReportTypes
    {
        Unknown,
        [EnumMember(Value = "ubpr")]
        UBPR,
        [EnumMember(Value = "call")]
        Call
    }

    public static class ReportType
    {
        public static ReportTypes Parse(string factName)
        {
            switch(factName.ToUpper())
            {
                case "UBPRC752":
                    return ReportTypes.UBPR;
                case "RCONC752":
                case "RCONA346":
                    return ReportTypes.Call;
                default:
                    return ReportTypes.Unknown;
            }
        }
    }
}
