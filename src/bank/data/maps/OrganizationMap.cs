using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class OrganizationMap : ClassMapper<Organization>
    {
        public OrganizationMap()
        {
            Map(x => x.OrganizationId).Key(KeyType.Identity);
            Map(x => x.Created).ReadOnly();
            Map(x => x.HasAvatar).Ignore();
            Map(x => x.ProfileUrl).Ignore();
            Map(x => x.WebsiteDomainOnly).Ignore();
            Map(x => x.WebsiteUri).Ignore();
            //Map(x => x.AvatarImageUrl).Ignore();

            AutoMap();
        }
    }
}
