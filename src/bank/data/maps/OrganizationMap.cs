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
            Map(x => x.HoldingCompany).Ignore();
            Map(x => x.Created).ReadOnly();
            Map(x => x.HasAvatar).Ignore();
            Map(x => x.ProfileUrl).Ignore();
            Map(x => x.WebsiteDomainOnly).Ignore();
            Map(x => x.WebsiteUri).Ignore();
            Map(x => x.CustomPeerGroups).Ignore();
            Map(x => x.StatePeerGroup).Ignore();
            Map(x => x.ReportImports).Ignore();
            Map(x => x.FilteredTransformations).Ignore();
            Map(x => x.SucessorTransformations).Ignore();
            Map(x => x.PredecessorTransformations).Ignore();
            Map(x => x.ChildRelationships).Ignore();
            Map(x => x.ParentRelationships).Ignore();
            Map(x => x.Status).Ignore();
            Map(x => x.Successor).Ignore();
            Map(x => x.FilteredChildRelationships).Ignore();
            Map(x => x.FilteredParentRelationships).Ignore();

            //Map(x => x.AvatarImageUrl).Ignore();

            AutoMap();
        }
    }
}
