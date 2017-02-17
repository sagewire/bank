using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class BranchFdicMap : ClassMapper<BranchFdic>
    {
        public BranchFdicMap()
        {

            Map(x => x.RSSDID).Key(KeyType.NotAKey);
            AutoMap();
        }
    }
}
