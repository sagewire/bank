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
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var reportItems = conn.Query<ReportImport>(" select  top(@BatchSize) * " +
                                                            "from   ReportImport " +
                                                            //"where OrganizationID in ('5535','5815','5572','1986','5246','5658','1226','4361','5944','5300','3179','5408','5283','2493','4234','4448','4175','4939','184','4152','5596','4868','5408','5556','5944','1226','5815','5246','5658','5230','4844','5157','5438','1598','4168','3562','2320','5118','740') " +
                                                            "where Processed is null and ReportType = 'ubpr' and State is null " +
                                                            //"   and Period >= '2012-01-01' " +
                                                            "order by Period desc ", // and Period >= '2012-03-31'",
                                                            new { BatchSize = batchSize },
                                                commandType: CommandType.Text);

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
