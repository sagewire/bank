using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.enums;

namespace bank.poco
{
    public class PeerGroupFact : Fact
    {
        public string PeerGroup { get; set; }

        public override FactTypes FactType
        {
            get
            {
                return FactTypes.PeerGroup;
            }
        }
    }
}
