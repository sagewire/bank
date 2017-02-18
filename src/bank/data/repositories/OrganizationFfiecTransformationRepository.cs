using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.extensions;
using bank.poco;
using Dapper;

namespace bank.data.repositories
{
    public class OrganizationFfiecTransformationRepository : BaseRepository<OrganizationFfiecTransformation>
    {
        public override OrganizationFfiecTransformation Get(OrganizationFfiecTransformation model)
        {
            var sql = new StringBuilder();
            sql.AppendLine("select * from OrganizationFfiecTransformation");
            sql.AppendLine("where ID_RSSD_PREDECESSOR = @ID_RSSD_PREDECESSOR");
            sql.AppendLine("and ID_RSSD_SUCCESSOR = @ID_RSSD_SUCCESSOR");
            sql.AppendLine("and D_DT_TRANS = @D_DT_TRANS");


            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var result = conn.QuerySingleOrDefault<OrganizationFfiecTransformation>(sql.ToString(), model,
                                                commandType: CommandType.Text);

                return result;

            }
        }

        public List<OrganizationFfiecTransformation> GetTransformationsForGraph()
        {
            var sql = new StringBuilder();
            sql.AppendLine("select * from OrganizationFfiecTransformation");
            //sql.AppendLine("where PredecessorOrganizationID is not null and SuccessorOrganizationID is not null");

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var org = conn.Query<OrganizationFfiecTransformation>(sql.ToString(),
                                                commandType: CommandType.Text);

                return org.ToList();
            }

        }
    }
}
