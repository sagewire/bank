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
        public Organization LookupByFFIEC(int id)
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


        public IList<Organization> GetOrganizations(IList<int> organizationIds)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var orgs = conn.Query<Organization>("select    * " +
                                              "from   Organization " +
                                              "where  OrganizationID in @OrganizationIDs ",
                                                new
                                                {
                                                    OrganizationIDs = organizationIds
                                                },
                                                commandType: CommandType.Text);

                return orgs.ToList();
            }
        }
    }
}
