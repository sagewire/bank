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

        public override void Save(Fact model)
        {
            if (model.FactType == enums.FactTypes.Company)
            {
                Save((CompanyFact)model);
            }
            else
            {
                throw new NotSupportedException();
            }
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
                    Period = model.Period,
                    Unit = model.Unit
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

        public IList<Fact> GetPeerGroupCustomFacts(IList<string> names, PeerGroupCustom peerGroupCustom, DateTime period, int? lookback = null)
        {
            if (!lookback.HasValue)
            {
                lookback = 0;
            }
            var periodStart = period.AddQuarters(-lookback.Value);

            return GetPeerGroupCustomFacts(names, peerGroupCustom, periodStart, period);
        }

        //exec GetAssetConcentrationPeerGroup 5815, '2016-09-30', 'UBPRE001,UBPRE002,UBPRE003,UBPRE004,UBPRE005,UBPRE006,UBPRE007,UBPRE008,UBPRE009,UBPRE010'
        public IList<Fact> GetPeerGroupCustomFacts(IList<string> names, PeerGroupCustom peerGroupCustom, DateTime periodStart, DateTime periodEnd)
        {
            const string sql = "select @peerGroup PeerGroup, Period, Name, avg(NumericValue) NumericValue, stdev(NumericValue), min(NumericValue) MinValue, max(NumericValue) MaxValue, min(Unit) Unit, count(*) c " +
                                "from Fact " +
                                "where OrganizationID in (select MemberOrganizationID from PeerGroupCustomMember pm where pm.PeerGroupCustomID = @peerGroupCustomID) " +
                               "    and Period between @periodStart and @periodEnd " +
                                "   and Name in @Names " +
                                "group by Period, Name ";

            IList<PeerGroupFact> facts;

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                //orgs is intentionally created as a string instead of a param due to some weird query plan decisions by SQL

                facts = conn.Query<PeerGroupFact>(sql, new
                {
                    PeerGroupCustomId = peerGroupCustom.PeerGroupCustomId,
                    Names = names.Distinct(),
                    PeriodEnd = periodEnd,
                    PeriodStart = periodStart,
                    PeerGroup = peerGroupCustom.PeerGroupCode
                },
                                                commandType: CommandType.Text)
                                                .ToList();

                var consolidatedFacts = ConsolidatePeerGroupFacts(facts, new string[] { peerGroupCustom.PeerGroupCode });

                return consolidatedFacts;

            }

            //using (var conn = new SqlConnection(Settings.ConnectionString))
            //{
            //    conn.Open();

            //    var facts = conn.Query<PeerGroupFact>(
            //        "GetPeerGroupCustomFacts",// @OrganizationID, @Period, @Names",
            //        new
            //        {
            //            Names = string.Join(",", names.Distinct()),
            //            PeerGroupCustomId = peerGroupCustom.PeerGroupCustomId,
            //            PeerGroupCode = peerGroupCustom.PeerGroupCode,
            //            PeriodStart = periodStart,
            //            PeriodEnd = periodEnd
            //        },
            //        commandType: CommandType.StoredProcedure)
            //        .ToList();

            //    var consolidatedFacts = ConsolidatePeerGroupFacts(facts, new string[] { peerGroupCustom.PeerGroupCode });

            //    return consolidatedFacts;
            //}
        }

        private IList<Fact> ConsolidatePeerGroupFacts(IList<PeerGroupFact> facts, IList<string> peerGroups)
        {
            if (facts == null || !facts.Any())
            {
                return new List<Fact>();
            }

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
                                                .ToDictionary(x => x.Period.Value, x => (Fact)x);

                    fact.HistoricalData = new SortedDictionary<DateTime, Fact>(d);
                }

                consolidatedFacts.AddRange(current);

            }

            return consolidatedFacts;
        }

        public IList<Fact> GetPeerGroupFacts(IList<string> names, IList<string> peerGroups, DateTime period, int? lookback = null)
        {
            if (!lookback.HasValue)
            {
                lookback = 0;
            }
            var periodStart = period.AddQuarters(-lookback.Value);

            return GetPeerGroupFacts(names, peerGroups, periodStart, period);
        }

        public IList<Fact> GetPeerGroupFacts(IList<string> names, IList<string> peerGroups, DateTime periodStart, DateTime periodEnd)
        {

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

        public IList<Fact> GetFacts(IList<string> names, DateTime period, int? lookback = null)
        {
            if (!lookback.HasValue)
            {
                lookback = 0;
            }
            var periodStart = period.AddQuarters(-lookback.Value);

            return GetFacts(names, periodStart, period);
        }

        public IList<Fact> GetFacts(IList<string> names, DateTime periodStart, DateTime periodEnd)
        {
            var sql = new StringBuilder();
            sql.AppendLine("select * from Fact");
            sql.AppendLine("where Name in @Names and Period between @PeriodStart");
            sql.AppendLine("and @PeriodEnd order by Period desc");

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                
                var facts = conn.Query<CompanyFact>(sql.ToString(), new
                                                {
                                                    Names = names.Distinct(),
                                                    PeriodEnd = periodEnd,
                                                    PeriodStart = periodStart
                                                },
                                                commandType: CommandType.Text)
                                                .ToList();

                return ConsolidateCompanyFacts(facts);
            }
        }

        private IList<Fact> ConsolidateCompanyFacts(IList<CompanyFact> facts)
        {
            var maxPeriod = facts.Select(x => x.Period.Value).Max();

            var orgs = new Dictionary<string, Fact>();

            var keyFormat = "{0}-{1}";

            foreach (var fact in facts)
            {
                Fact combinedFact;
                var key = string.Format(keyFormat, fact.OrganizationId, fact.Name);
                if (orgs.ContainsKey(key))
                {
                    combinedFact = orgs[key];
                    combinedFact.HistoricalData.Add(fact.Period.Value, fact);
                }
                else
                {
                    orgs.Add(key, fact);
                }
            }

            var consolidatedFacts = orgs.Values.ToList();

            //var organizationIds = facts.Select(x => x.OrganizationId).Distinct().ToList();

            //foreach (var orgId in organizationIds.Distinct())
            //{
            //    //var maxPeriod = facts.Where(x => x.OrganizationId == orgId).Select(x => x.Period.Value).Max();
            //    //pack older facts into most recent fact
            //    var current = facts.Where(x => x.Period == maxPeriod && x.OrganizationId == orgId).ToList();

            //    foreach (var fact in current)
            //    {
            //        var d = facts.Where(x => x.Name == fact.Name &&
            //                                    x.Period <= fact.Period &&
            //                                    x.OrganizationId == orgId &&
            //                                    x.NumericValue.HasValue)
            //                                    .ToDictionary(x => x.Period.Value, x => (Fact)x);

            //        fact.HistoricalData = new SortedDictionary<DateTime, Fact>(d);
            //    }

            //    consolidatedFacts.AddRange(current);

            //}

            return consolidatedFacts;
        }

        public IList<Fact> GetFacts(IList<string> names, IList<int> organizationIds, DateTime period, int? lookback = null)
        {
            if (!lookback.HasValue)
            {
                lookback = 0;
            }
            var periodStart = period.AddQuarters(-lookback.Value);

            return GetFacts(names, organizationIds, periodStart, period);
        }

        public IList<Fact> GetFacts(IList<string> names, IList<int> organizationIds, DateTime periodStart, DateTime periodEnd)
        {
            //var periodEnd = SetPeriodEnd(period);

            //var periodStart = lookback.HasValue ? lookback.Value : new DateTime(1900, 1, 1);

            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var orgs = string.Join(",", organizationIds.Distinct());
                //orgs is intentionally created as a string instead of a param due to some weird query plan decisions by SQL

                var facts = conn.Query<CompanyFact>("  select  * " +
                                                "from   Fact " +
                                                "where OrganizationID in @OrganizationIds " + //(" + orgs + ") " +
                                                "   and Period between @periodStart and @periodEnd " +
                                                "   and Name in @Names ", new
                                                {
                                                    Names = names.Distinct(),
                                                    PeriodEnd = periodEnd,
                                                    PeriodStart = periodStart,
                                                    OrganizationIds = organizationIds.Distinct()
                                                },
                                                commandType: CommandType.Text)
                                                .ToList();

                if (!facts.Any())
                {
                    return new List<Fact>();
                }


                var maxPeriod = facts.Select(x => x.Period.Value).Max();

                var consolidatedFacts = new List<Fact>();

                foreach (var orgId in organizationIds.Distinct())
                {
                    //var maxPeriod = facts.Where(x => x.OrganizationId == orgId).Select(x => x.Period.Value).Max();
                    //pack older facts into most recent fact
                    var current = facts.Where(x => x.Period == maxPeriod && x.OrganizationId == orgId).ToList();

                    foreach (var fact in current)
                    {
                        var d = facts.Where(x => x.Name == fact.Name &&
                                                    x.Period <= fact.Period &&
                                                    x.OrganizationId == orgId &&
                                                    x.NumericValue.HasValue)
                                                    .ToDictionary(x => x.Period.Value, x => (Fact)x);

                        fact.HistoricalData = new SortedDictionary<DateTime, Fact>(d);
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

        //public IList<CompanyFact> GetReports(int organizationId)
        //{
        //    var names = new string[]
        //    {
        //        "RCONC752",
        //        "RCONA346",
        //        "UBPRC752"
        //    };

        //    using (var conn = new SqlConnection(Settings.ConnectionString))
        //    {
        //        conn.Open();
        //        var facts = conn.Query<CompanyFact>("  select  * " +
        //                                        "from   Fact " +
        //                                        "where  OrganizationID = @OrganizationID " +
        //                                        "   and Name in @Names " +
        //                                        "order by Period desc"
        //                                        , new
        //                                        {
        //                                            Names = names,
        //                                            OrganizationID = organizationId
        //                                        },
        //                                        commandType: CommandType.Text);

        //        return facts.ToList();
        //    }
        //}

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
