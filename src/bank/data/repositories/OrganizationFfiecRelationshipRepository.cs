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
    public class OrganizationFfiecRelationshipRepository : BaseRepository<OrganizationFfiecRelationship>
    {
        public override OrganizationFfiecRelationship Get(OrganizationFfiecRelationship model)
        {
            var sql = new StringBuilder();
            sql.AppendLine("select * from OrganizationFfiecRelationship");
            sql.AppendLine("where ID_RSSD_Parent = @ID_RSSD_Parent");
            sql.AppendLine("and ID_RSSD_Offspring = @ID_RSSD_Offspring");
            sql.AppendLine("and ctrl_ind = @ctrl_ind");
            sql.AppendLine("and equity_ind = @equity_ind");
            sql.AppendLine("and other_basis_ind = @other_basis_ind");
            sql.AppendLine("and D_DT_START = @D_DT_START");


            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var result = conn.QuerySingleOrDefault<OrganizationFfiecRelationship>(sql.ToString(), model,
                                                commandType: CommandType.Text);

                return result;

            }
        }
    }
}
