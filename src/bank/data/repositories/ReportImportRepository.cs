using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using bank.poco;

namespace bank.data.repositories
{
    public class ReportImportRepository : BaseRepository<ReportImport>
    {
        public override ReportImport Get(ReportImport model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var fact = conn.QuerySingleOrDefault<ReportImport>("  select  * " +
                                                "from   ReportImport " +
                                                "where OrganizationID = @OrganizationID " +
                                                "   and ReportType = @ReportType " +
                                                "   and Period = @Period ", new
                                                {
                                                    OrganizationID = model.OrganizationId,
                                                    ReportType = model.ReportType.ToString(),
                                                    Period = model.Period
                                                },
                                                commandType: CommandType.Text);

                return fact;

            }
        }

        public override void Update(ReportImport model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var reportItems = conn.Execute(" update ReportImport " +
                                                "set Filed = @Filed, Processed = @Processed, State = @State " +
                                                "where OrganizationID = @OrganizationID and ReportType = @ReportType and Period = @Period",
                                                new
                                                {
                                                    OrganizationID = model.OrganizationId,
                                                    ReportType = model.ReportTypeAsString,
                                                    Period = model.Period,
                                                    State = model.State,
                                                    Filed = model.Filed,
                                                    Processed = model.Processed
                                                },
                                                commandType: CommandType.Text);


            }
        }

        public IList<ReportImport> GetRecordsToImport(int batchSize)
        {
            var sql = new StringBuilder();
            sql.AppendLine("select  top(@BatchSize) *");
            sql.AppendLine("from   ReportImport ");
            sql.AppendLine("where Processed is null");
            //sql.AppendLine("and ReportType = 'ubpr' ");
            sql.AppendLine("and OrganizationID in (select distinct OrganizationID from UserFavorite where FavoriteType = 100 or Visits > 15)");
            sql.AppendLine("and Period = '2016-12-31'");
            sql.AppendLine("and State is null");
            sql.AppendLine("order by ReportType, Period desc");
            
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var reportItems = conn.Query<ReportImport>(sql.ToString(), new { BatchSize = batchSize }, commandType: CommandType.Text);

                return reportItems.ToList();

            }
        }

        public DateTime? LastReportDate()
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var lastDownload = conn.ExecuteScalar<DateTime?>("select max(Period) from ReportImport",
                                                commandType: CommandType.Text);

                return lastDownload;

            }
        }

        public override void Save(ReportImport model)
        {
            var existing = Get(model);

            if (existing != null)
            {
                Update(model);
            }
            else
            {
                Insert(model);
            }
        }
    }
}
