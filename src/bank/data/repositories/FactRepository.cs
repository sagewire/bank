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

        static FactRepository()
        {
            Dapper.SqlMapper.AddTypeMap(typeof(string), System.Data.DbType.AnsiString);
        }

        public void Save(CompanyFact model)
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

        private DateTime SetPeriodEnd(DateTime? period)
        {
            var periodEnd = DateTime.Now;

            if (period.HasValue && period.Value.Year >= 2000)
            {
                periodEnd = period.Value;
            }
            else
            {
                periodEnd = DateTime.Now;
            }

            return periodEnd;
        }

        //exec GetAssetConcentrationPeerGroup 5815, '2016-09-30', 'UBPRE001,UBPRE002,UBPRE003,UBPRE004,UBPRE005,UBPRE006,UBPRE007,UBPRE008,UBPRE009,UBPRE010'
        public IList<Fact> GetAssetConPeerGroupFacts(IList<string> names, int organizationId, DateTime? period = null, DateTime? lookback = null)
        {
            var periodStart = lookback.HasValue ? lookback.Value : new DateTime(1900, 1, 1);

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                var facts = conn.Query<PeerGroupFact>(
                    "GetAssetConcentrationPeerGroup @OrganizationID, @Period, @Names",
                    new {
                        Names = names,
                        OrganizationId = organizationId,
                        Period = period.Value
                    },
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                var consolidatedFacts = ConsolidatePeerGroupFacts(facts, new string[] { "AssetCon" });

                return consolidatedFacts;
            }
        }

        private IList<Fact> ConsolidatePeerGroupFacts(IList<PeerGroupFact> facts, IList<string> peerGroups)
        {
            var maxPeriod = facts.Select(x => x.Period.Value).Max();

            var consolidatedFacts = new List<Fact>();

            foreach (var peerGroup in peerGroups.Distinct())
            {
                //pack older facts into most recent fact
                var current = facts.Where(x => x.Period == maxPeriod && x.PeerGroup == peerGroup).ToList();

                foreach (var fact in current)
                {
                    var d = facts.Where(x => x.Name == fact.Name &&
                                                x.Period <= fact.Period &&
                                                x.PeerGroup == peerGroup &&
                                                x.NumericValue.HasValue)
                                                .ToDictionary(x => x.Period.Value, x => x.NumericValue.Value);

                    fact.HistoricalData = new SortedDictionary<DateTime, decimal>(d);
                }

                consolidatedFacts.AddRange(current);

            }

            return consolidatedFacts;
        }

        public IList<Fact> GetPeerGroupFacts(IList<string> names, IList<string> peerGroups, DateTime? period = null, DateTime? lookback = null)
        {
            var periodEnd = SetPeriodEnd(period);

            var periodStart = lookback.HasValue ? lookback.Value : new DateTime(1900, 1, 1);

            IList<PeerGroupFact> facts;

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                //orgs is intentionally created as a string instead of a param due to some weird query plan decisions by SQL

                facts = conn.Query<PeerGroupFact>("  select  * " +
                                                "from   PeerGroupFact " +
                                                "where PeerGroup in @PeerGroups " +
                                                "   and Period between @periodStart and @periodEnd " +
                                                "   and Name in @Names ", new
                                                {
                                                    PeerGroups = peerGroups.Distinct(),
                                                    Names = names.Distinct(),
                                                    PeriodEnd = periodEnd,
                                                    PeriodStart = periodStart
                                                },
                                                commandType: CommandType.Text)
                                                .ToList();
                
                var consolidatedFacts = ConsolidatePeerGroupFacts(facts, peerGroups);

                return consolidatedFacts;

            }


        }

        public IList<Fact> GetFacts(IList<string> names, IList<int> organizationIds, DateTime? period = null, DateTime? lookback = null)
        {
            var periodEnd = SetPeriodEnd(period);

            var periodStart = lookback.HasValue ? lookback.Value : new DateTime(1900, 1, 1);

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var orgs = string.Join(",", organizationIds.Distinct());
                //orgs is intentionally created as a string instead of a param due to some weird query plan decisions by SQL

                var facts = conn.Query<CompanyFact>("  select  * " +
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

        public IList<CompanyFact> GetFacts(string name, int organizationId)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var facts = conn.Query<CompanyFact>("GetFacts", new
                {
                    Name = name,
                    OrganizationID = organizationId
                },
                commandType: CommandType.StoredProcedure);

                return facts.ToList();
            }
        }

        public IList<CompanyFact> GetReports(int organizationId)
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
                var facts = conn.Query<CompanyFact>("  select  * " +
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
