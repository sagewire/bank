using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class FactMapper : ClassMapper<Fact>
    {
        public FactMapper()
        {
            Map(x => x.OrganizationId).Key(KeyType.NotAKey);
            AutoMap();
        }
    }
}
