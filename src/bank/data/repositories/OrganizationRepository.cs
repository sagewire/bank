﻿using System;
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
                var predicate = Predicates.Field<Organization>(x => x.FFIEC, Operator.Eq, id);

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
            const string reportListSql = "select * from ReportImport where OrganizationID in @OrganizationIds and Processed is not null";

            var sb = new StringBuilder();

            sb.AppendLine(orgSql);

            if (loadPeerGroups)
            {
                sb.AppendLine(peerSql);
                sb.AppendLine(peerMemberSql);
            }

            if (loadReportList)
            {
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
                        var reportList = multi.Read<ReportImport>().ToList();

                        foreach (var org in orgs)
                        {
                            org.ReportImports = reportList.Where(x => x.OrganizationId == org.OrganizationId).ToList();
                        }
                    }

                    return orgs;
                }
            }
        }
    }
}