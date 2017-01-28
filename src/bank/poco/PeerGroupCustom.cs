using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.poco
{
    public class PeerGroupCustom
    {
        public int PeerGroupCustomId { get; set; }
        public string PeerGroupCode { get; set; }
        public string PeerGroupLabel { get; set; }
        public int OrganizationId { get; set; }

        public IList<PeerGroupCustomMember> Members { get; set; }
    }
}
