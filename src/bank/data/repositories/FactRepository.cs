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
    public class FactRepository : BaseRepository<Fact>
    {
        public override void Save(Fact model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                conn.Execute("SetFact", new
                {
                    Name = model.Name,
                    NumericValue = model.NumericValue,
                    Value = model.Value,
                    OrganizationID = model.OrganizationId,
                    Period = model.Period
                },
                commandType: CommandType.StoredProcedure);
            }
        }

        public IList<Fact> GetPeerFacts(IList<string> names, IList<string> peers, DateTime? period = null, DateTime? lookback = null)
        {

        }

        public IList<Fact> GetFacts(IList<string> names, IList<int> organizationIds, DateTime? period = null, DateTime? lookback = null)
        {
            
            Dapper.SqlMapper.AddTypeMap(typeof(string), System.Data.DbType.AnsiString);
            var periodEnd = DateTime.Now;

            if (period.HasValue && period.Value.Year >= 2000)
            {
                periodEnd = period.Value;
            }
            else
            {
                periodEnd = DateTime.Now;
            }

            var periodStart = lookback.HasValue ? lookback.Value : new DateTime(1900, 1, 1);

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var orgs = string.Join(",", organizationIds.Distinct());
                //orgs is intentionally created as a string instead of a param due to some weird query plan decisions by SQL

                var facts = conn.Query<Fact>("  select  * " +
                                                "from   Fact " +
                                                "where OrganizationID in (" + orgs + ") " +
                                                "   and Period between @periodStart and @periodEnd " +
                                                "   and Name in @Names ", new
                                                {
                                                    Names = names.Distinct(),
                                                    PeriodEnd = periodEnd,
                                                    PeriodStart = periodStart
                                                },
                                                commandType: CommandType.Text)
                                                .ToList();


                var maxPeriod = facts.Select(x => x.Period.Value).Max();

                var consolidatedFacts = new List<Fact>();

                foreach (var orgId in organizationIds.Distinct())
                {
                    //pack older facts into most recent fact
                    var current = facts.Where(x => x.Period == maxPeriod && x.OrganizationId == orgId).ToList();

                    foreach (var fact in current)
                    {
                        var d = facts.Where(x => x.Name == fact.Name &&
                                                    x.Period <= fact.Period &&
                                                    x.OrganizationId == orgId &&
                                                    x.NumericValue.HasValue)
                                                    .ToDictionary(x => x.Period.Value, x => x.NumericValue.Value);

                        fact.HistoricalData = new SortedDictionary<DateTime, decimal>(d);
                    }

                    consolidatedFacts.AddRange(current);

                }

                return consolidatedFacts;
            }
        }


        //public IList<Fact> GetFacts(string[] names, int[] organizationIds)
        //{
        //    using (var conn = new SqlConnection(Settings.ConnectionString))
        //    {
        //        conn.Open();
        //        var facts = conn.Query<Fact>("  select    * " +
        //                                        "from   Fact " +
        //                                        "where  OrganizationID in @OrganizationIDs " +
        //                                        "   and Name in @Names", new
        //                                        {
        //                                            Names = names,
        //                                            OrganizationIDs = organizationIds
        //                                        },
        //                                        commandType: CommandType.Text);

        //        return facts.ToList();
        //    }
        //}

        public IList<Fact> GetFacts(string name, int organizationId)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var facts = conn.Query<Fact>("GetFacts", new
                {
                    Name = name,
                    OrganizationID = organizationId
                },
                commandType: CommandType.StoredProcedure);

                return facts.ToList();
            }
        }

        public IList<Fact> GetReports(int organizationId)
        {
            var names = new string[]
            {
                "RCONC752",
                "RCONA346",
                "UBPRC752"
            };

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var facts = conn.Query<Fact>("  select  * " +
                                                "from   Fact " +
                                                "where  OrganizationID = @OrganizationID " +
                                                "   and Name in @Names " +
                                                "order by Period desc"
                                                , new
                                                {
                                                    Names = names,
                                                    OrganizationID = organizationId
                                                },
                                                commandType: CommandType.Text);

                return facts.ToList();
            }
        }

        //public void PopulateChart(ChartConfig configuration, DateTime? currentPeriod = null)
        //{
        //    PopulateCharts(new ChartConfig[] { configuration }, currentPeriod);
        //}

        //public void PopulateCharts(ChartConfig[] configurations, DateTime? currentPeriod = null)
        //{
        //    var factNames = new List<string>();
        //    var orgs = new List<int>();

        //    foreach (var config in configurations)
        //    {
        //        factNames.AddRange(config.TrendSeries.Select(x => x.FactName));
        //        orgs.Add(config.PrimaryOrganizationId);

        //        foreach (var series in config.Series)
        //        {
        //            var companySeries = series as CompanyFactTrendSeries;

        //            if (companySeries != null)
        //            {
        //                factNames.Add(companySeries.FactName);
        //                orgs.Add(companySeries.OrganizationId);
        //            }
        //        }
        //    }

        //    var data = GetFacts(factNames.Distinct().ToArray(), orgs.ToArray(), currentPeriod);

        //    foreach (var config in configurations)
        //    {
        //        foreach (var seriesConfig in config.TrendSeries)
        //        {
        //            Fact seriesData;

        //            var companySeries = seriesConfig as CompanyFactTrendSeries;

        //            if (companySeries != null)
        //            {
        //                seriesData = data.SingleOrDefault(x => seriesConfig.FactName.ToUpper() == x.Name &&
        //                                            x.OrganizationId == companySeries.OrganizationId);
        //            }
        //            else
        //            {
        //                seriesData = data.SingleOrDefault(x => seriesConfig.FactName == x.Name &&
        //                                            x.OrganizationId == config.PrimaryOrganizationId);
        //            }


        //            if (seriesConfig.SeriesType != SeriesType.Pie && seriesData != null)
        //            {
        //                seriesConfig.X = seriesData.HistoricalData.Keys.ToList();
        //                seriesConfig.Y = seriesData.HistoricalData.Values.ToList();
        //            }
        //            else
        //            {
        //                seriesConfig.X = new List<DateTime>();
        //                seriesConfig.Y = new List<decimal>();
        //            }

        //            if (seriesData != null)
        //            {
        //                //seriesConfig.X.Add(seriesData.Period.Value);
        //                //seriesConfig.Y.Add(seriesData.NumericValue.Value);

        //                seriesConfig.Fact = seriesData;
        //            }
        //        }

        //    }
        //}

    }
}
