using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using Dapper;
using DapperExtensions;

namespace bank.data.repositories
{
    public class OrganizationRepository : BaseRepository<Organization>
    {
        public Organization LookupByRssd(int id)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var predicate = Predicates.Field<Organization>(x => x.ID_RSSD, Operator.Eq, id);

                var results = conn.GetList<Organization>(predicate);

                if (results.Any())
                {
                    return results.First();
                }

                return null;
            }
        }

        public Organization NextTwitterFriendUpdate()
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var org = conn.QuerySingleOrDefault<Organization>("select top 1 * " +
                                              "from   Organization " +
                                              "where Twitter is not null " +
                                              "order by TwitterFriendUpdate ",
                                                commandType: CommandType.Text);

                return org;
            }
        }

        public List<Organization> GetOrganizationsForGraph()
        {
            var sql = new StringBuilder();
            //sql.AppendLine("select * from Organization where OrganizationID in (");
            //sql.AppendLine("select  PredecessorOrganizationID from OrganizationFfiecTransformation where PredecessorOrganizationID is not null");
            //sql.AppendLine("union");
            //sql.AppendLine("select SuccessorOrganizationID from OrganizationFfiecTransformation where SuccessorOrganizationID is not null)");

            sql.AppendLine("select * from Organization");

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var org = conn.Query<Organization>(sql.ToString(),
                                                commandType: CommandType.Text);

                return org.ToList();
            }
        }

        public override void Update(Organization model)
        {
            base.Update(model);
        }

        public override void Save(Organization model)
        {
            var existing = Get(model.OrganizationId);

            if (existing == null)
            {
                Insert(model);
            }
            else
            {
                Update(model);
            }
        }

        public Organization GetOrganization(int organizationId, bool loadPeerGroups = false, bool loadReportList = false)
        {
            return GetOrganizations(new int[] { organizationId }, loadPeerGroups, loadReportList)?.First();
        }

        public IList<Organization> GetOrganizations(IList<int> organizationIds, bool loadPeerGroups = false, bool loadReportList = false)
        {
            const string orgSql = "select * from Organization where OrganizationID in @OrganizationIDs ";
            const string peerSql = "select * from PeerGroupCustom where OrganizationID in @OrganizationIDs ";
            const string peerMemberSql = "select * from PeerGroupCustomMember where PeerGroupCustomID in (select PeerGroupCustomID from PeerGroupCustom where OrganizationID in @OrganizationIDs) ";
            const string transformList = "select * from OrganizationFfiecTransformation where SuccessorOrganizationID in @OrganizationIds or PredecessorOrganizationID in @OrganizationIds order by D_DT_TRANS desc";
            const string transformOrgList = "select * from Organization where OrganizationID in ( select PredecessorOrganizationID from OrganizationFfiecTransformation where SuccessorOrganizationID in @OrganizationIds union select SuccessorOrganizationId from OrganizationFfiecTransformation where PredecessorOrganizationID in @OrganizationIds)";
            const string reportListSql = "select * from ReportImport where OrganizationID in @OrganizationIds and Processed is not null";
            const string relationshipChildrenSql = "select * from OrganizationFfiecRelationship where ParentOrganizationID in @OrganizationIds";
            const string relationshipChildrenOrgSql = "select * from Organization where OrganizationID in ( select OffspringOrganizationID from OrganizationFfiecRelationship where ParentOrganizationID in @OrganizationIds )";
            const string relationshipParentSql = "select * from OrganizationFfiecRelationship where OffspringOrganizationID in @OrganizationIds";
            const string relationshipParentOrgSql = "select * from Organization where OrganizationID in ( select ParentOrganizationID from OrganizationFfiecRelationship where OffspringOrganizationID in @OrganizationIds )";

            var sb = new StringBuilder();

            sb.AppendLine(orgSql);

            if (loadPeerGroups)
            {
                sb.AppendLine(peerSql);
                sb.AppendLine(peerMemberSql);
            }

            if (loadReportList)
            {
                sb.AppendLine(transformList);
                sb.AppendLine(transformOrgList);
                sb.AppendLine(relationshipChildrenSql);
                sb.AppendLine(relationshipChildrenOrgSql);
                sb.AppendLine(relationshipParentSql);
                sb.AppendLine(relationshipParentOrgSql);
                sb.AppendLine(reportListSql);
            }

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                using (SqlMapper.GridReader multi = conn.QueryMultiple(sb.ToString(), new { OrganizationIDs = organizationIds },
                    commandTimeout: 100, commandType: CommandType.Text))
                {
                    //get entities
                    var orgs = multi.Read<Organization>().ToList();

                    if (loadPeerGroups)
                    {
                        var peerGroups = multi.Read<PeerGroupCustom>().ToList();
                        var peerGroupMembers = multi.Read<PeerGroupCustomMember>().ToList();

                        foreach (var peerGroup in peerGroups)
                        {
                            peerGroup.Members = peerGroupMembers.Where(x => x.PeerGroupCustomId == peerGroup.PeerGroupCustomId).ToList();
                        }

                        foreach (var org in orgs)
                        {
                            org.CustomPeerGroups = peerGroups.Where(x => x.OrganizationId == org.OrganizationId).ToList();
                        }
                    }

                    if (loadReportList)
                    {
                        var transformations = multi.Read<OrganizationFfiecTransformation>().ToList();
                        var transformOrgs = multi.Read<Organization>().ToList();
                        var relationshipsChildren = multi.Read<OrganizationFfiecRelationship>().ToList();
                        var relationshipsChildrenOrgs = multi.Read<Organization>().ToList();
                        var relationshipsParent = multi.Read<OrganizationFfiecRelationship>().ToList();
                        var relationshipsParentOrgs = multi.Read<Organization>().ToList();

                        var reportList = multi.Read<ReportImport>().ToList();

                        foreach (var transformation in transformations)
                        {
                            transformation.PredecessorOrganization = transformOrgs.SingleOrDefault(x => x.OrganizationId == transformation.PredecessorOrganizationId);
                            transformation.SuccessorOrganization = transformOrgs.SingleOrDefault(x => x.OrganizationId == transformation.SuccessorOrganizationId);

                        }

                        foreach (var relationship in relationshipsChildren)
                        {
                            relationship.OffspringOrganization = relationshipsChildrenOrgs.SingleOrDefault(x => x.OrganizationId == relationship.OffspringOrganizationId);
                        }

                        foreach (var relationship in relationshipsParent)
                        {
                            relationship.ParentOrganization = relationshipsParentOrgs.SingleOrDefault(x => x.OrganizationId == relationship.ParentOrganizationId);
                        }

                        foreach (var org in orgs)
                        {
                            org.SucessorTransformations = transformations.Where(x => x.SuccessorOrganizationId == org.OrganizationId).ToList();
                            org.PredecessorTransformations = transformations.Where(x => x.PredecessorOrganizationId == org.OrganizationId).ToList();
                            org.ChildRelationships = relationshipsChildren.Where(x => x.ParentOrganizationId == org.OrganizationId).OrderByDescending(x=>x.D_DT_END).ToList();
                            org.ParentRelationships = relationshipsParent.Where(x => x.OffspringOrganizationId == org.OrganizationId).OrderByDescending(x=>x.D_DT_END).ToList();
                            org.ReportImports = reportList.Where(x => x.OrganizationId == org.OrganizationId).ToList();
                        }
                    }

                    return orgs;
                }
            }
        }
    }
}
