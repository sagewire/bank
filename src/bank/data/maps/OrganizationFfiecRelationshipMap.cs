using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class OrganizationFfiecRelationshipMap : ClassMapper<OrganizationFfiecRelationship>
    {
        public OrganizationFfiecRelationshipMap()
        {
            Map(x => x.ParentOrganization).Ignore();
            Map(x => x.OffspringOrganization).Ignore();
            Map(x => x.DateRelationshipStart).Ignore();
            Map(x => x.DateRelationshipEnd).Ignore();
            
            AutoMap();

        }
    }
}
