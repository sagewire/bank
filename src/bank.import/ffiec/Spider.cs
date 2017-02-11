using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JeffFerguson.Gepsio;
using bank.data.repositories;
using bank.enums;
using System.Text.RegularExpressions;
using bank.poco;
using Newtonsoft.Json;
using System.Xml.Linq;
using Microsoft.Web.Services3.Security.Tokens;
using bank.extensions;

namespace bank.import.ffiec
{
    public class Spider
    {
        private static Regex _id = new Regex(@"(?<=\s)\d+?(?=\(ID RSSD\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static TaskPool<TaskData> _taskPool = new TaskPool<TaskData>();
        private static bool _initialImport = false;

        public static void Start()
        {
            Console.WriteLine("starting download");

            InitializeTaskPool();

            _taskPool.Start();

            var repo = new ReportImportRepository();

            var lastRun = repo.LastReportDate();
            var periodStart = DateTime.Now.LastQuarterDate();
            var periodEnd = DateTime.Parse("2002-12-31");
            //var periodStart = DateTime.Parse("2008-03-31");

            if (!lastRun.HasValue)
            {
                lastRun = periodEnd;
            }

            //lastRun = periodStart;

            while (periodStart >= periodEnd)
            {
                _taskPool.Enqueue(new TaskData { period = periodStart, lastRun = lastRun.Value });
                periodStart = periodStart.AddQuarters(-1);
            }

        }
        static void RetrieveFilers(DateTime period, DateTime lastRun)
        {
            var userToken = new UsernameToken(Settings.FfiecWebServiceUsername, Settings.FfiecWebServiceToken, PasswordOption.SendHashed);
            var ffiec = new gov.ffiec.cdr.RetrievalService();

            ffiec.RequestSoapContext.Security.Tokens.Add(userToken);

            var result = ffiec.RetrieveFilersSubmissionDateTime(
                gov.ffiec.cdr.ReportingDataSeriesName.Call,
                period.ToString(),
                lastRun.ToString());

            //var result = ffiec.RetrieveFacsimile(
            //    gov.ffiec.cdr.ReportingDataSeriesName.Call,
            //    period,
            //    gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD,
            //    688556,
            //    gov.ffiec.cdr.FacsimileFormat.XBRL);

            //var result = ffiec.RetrieveUBPRXBRLFacsimile("2002-12-31", gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD, 688556);

            foreach (var item in result)
            {
                var orgRepo = new OrganizationRepository();
                var org = orgRepo.LookupByRssd(item.ID_RSSD);

                if (org != null)
                {
                    var callReportItem = new ReportImport();
                    callReportItem.Filed = DateTime.Parse(item.DateTime);
                    callReportItem.Period = period;
                    callReportItem.ReportType = ReportTypes.Call;
                    callReportItem.OrganizationId = org.OrganizationId;

                    var ubprReportItem = new ReportImport();
                    ubprReportItem.Filed = DateTime.Parse(item.DateTime);
                    ubprReportItem.Period = period;
                    ubprReportItem.ReportType = ReportTypes.UBPR;
                    ubprReportItem.OrganizationId = org.OrganizationId;


                    //if (_initialImport)
                    //{
                    //    Console.WriteLine("Inserting {0} {1} {2}", callReportItem.Period, callReportItem.ReportTypeAsString, callReportItem.OrganizationId);
                    //    Repository<ReportImport>.New().Insert(callReportItem);
                    //    Repository<ReportImport>.New().Insert(ubprReportItem);
                    //}
                    //else
                    //{

                    var existingCall = Repository<ReportImport>.New().Get(callReportItem);

                    if (existingCall != null)
                    {
                        if (existingCall.Filed != callReportItem.Filed)
                        {
                            Console.WriteLine("Updated filing {0} {1} {2}", callReportItem.Period, callReportItem.ReportTypeAsString, callReportItem.OrganizationId);
                            Repository<ReportImport>.New().Update(callReportItem);
                            Repository<ReportImport>.New().Update(ubprReportItem);
                        }
                        else
                        {
                            Console.WriteLine("No change {0} {1} {2}", callReportItem.Period, callReportItem.ReportTypeAsString, callReportItem.OrganizationId);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Inserting {0} {1} {2}", callReportItem.Period, callReportItem.ReportTypeAsString, callReportItem.OrganizationId);
                        Repository<ReportImport>.New().Insert(callReportItem);
                        Repository<ReportImport>.New().Save(ubprReportItem);
                    }

                }

                //}
            }

        }

        static void InitializeTaskPool()
        {
            _taskPool.MaxWorkers = 4;

            _taskPool.RefillQueue += _taskPool_RefillQueue;
            _taskPool.NextTask += _taskPool_NextTask;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty; ;

        }

        private static int _taskPool_RefillQueue(int queueCount)
        {
            return 0;
        }

        private static void _taskPool_NextTask(TaskData task)
        {
            Console.WriteLine("Next Spider Task period: {0}\tlast run:{1}", task.period, task.lastRun);
            //ProcessFile(task);
            RetrieveFilers(task.period, task.lastRun);

        }

        private static void _taskPool_QueueEmpty()
        {
            Console.WriteLine("Queue empty");
        }

        static Organization GetOrg(int id)
        {
            var orgRepo = new OrganizationRepository();
            var org = orgRepo.LookupByRssd(id);

            if (org == null)
            {
                org = new Organization
                {
                    Name = "",
                    FFIEC = id
                };

                Repository<Organization>.New().Insert(org);
            }

            return org;
        }

        static void WriteTemplate(PresentableFactTree tree)
        {
            var sb = new StringBuilder();
            var branch = tree.TopLevelNodes.First();

            WriteLine(sb, branch);

            Console.WriteLine(sb.ToString());
        }

        static void WriteLine(StringBuilder sb, PresentableFactTreeNode node, int level = 0)
        {
            var note = "";

            for (int i = 0; i <= level; i++)
            {
                note += ".o";
            }

            note = note.Substring(1);

            var line = string.Format("{0}\t{1}\t{2}\t{3}",
                                    note,
                                    node.NodeLabel,
                                    node.Name,
                                    node.IsAbstract
                                    );

            sb.AppendLine(line);
            Console.WriteLine(line);

            foreach (var child in node.ChildNodes)
            {
                WriteLine(sb, child, level + 1);
            }
        }

        static void Serialize(PresentableFactTree tree)
        {
            XElement root = new XElement("report");
            SerializeNode(root, tree.TopLevelNodes[0]);

            root.Save(@"call-report041-2016-09-30-v129.xml");
        }

        static void SerializeNode(XElement element, PresentableFactTreeNode node)
        {
            var child = new XElement("line");
            child.Add(new XAttribute("name", node.Name));
            child.Add(new XAttribute("label", node.NodeLabel));
            child.Add(new XAttribute("abstract", node.IsAbstract));

            element.Add(child);

            foreach (var childnode in node.ChildNodes)
            {
                SerializeNode(child, childnode);
            }
        }

        static void ProcessFile(string file)
        {
            //var contents = File.ReadAllText(file);
            //contents = contents.Replace("ubpr-v61", "ubpr-v64");
            ////contents = contents.Replace("/v61/", "/v64/");
            //File.WriteAllText(file, contents);


            var id = int.Parse(_id.Match(file).Value);

            var org = GetOrg(id);
            var moveFolder = "completed";

            XbrlDocument doc = new XbrlDocument();
            Item lastFact;
            try
            {
                doc.Load(file);

                //var tree = doc.XbrlFragments[0].GetPresentableFactTree();

                //                Serialize(tree);

                //WriteTemplate(tree);

                if (!doc.IsValid)
                {
                    //throw new Exception(string.Format("Invalid XBRL {0}", file));
                }

                foreach (Item fact in doc.XbrlFragments[0].Facts)//.Where(x=>x.Name == "RIAD4300"))
                {
                    lastFact = fact;
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", System.Threading.Thread.CurrentThread.ManagedThreadId, fact.Id, fact.Name, fact.Value);

                    decimal numeric;
                    decimal? numericValue = null;

                    if (decimal.TryParse(fact.Value, out numeric))
                    {
                        numericValue = numeric;
                    }

                    DateTime? period = null;
                    if (fact.ContextRef.InstantDate != null && fact.ContextRef.InstantDate > DateTime.MinValue)
                    {
                        period = fact.ContextRef.InstantDate;
                    }
                    else
                    {
                        period = GetDate(fact.ContextRef.PeriodEndDate);
                    }

                    var factObj = new bank.poco.CompanyFact
                    {
                        OrganizationId = org.OrganizationId,
                        Name = fact.Name,
                        NumericValue = numericValue,
                        Value = fact.Value,
                        Period = period,
                        Unit = fact.UnitRefName?.ToUpper().First()
                    };

                    Repository<bank.poco.Fact>.New().Save(factObj);

                }

            }
            catch (Exception e)
            {
                doc = null;
                moveFolder = "error";

                Console.WriteLine(e);
            }

            var filename = Path.GetFileName(file);
            var movePath = Path.Combine(Path.GetDirectoryName(file), moveFolder);
            movePath = Path.Combine(movePath, filename);



            File.Move(file, movePath);
        }

        static DateTime? GetDate(DateTime date)
        {
            if (date > DateTime.MinValue)
            {
                return date;
            }
            else
            {
                return null;
            }
        }
    }

    internal class TaskData
    {
        public DateTime period { get; set; }
        public DateTime lastRun { get; set; }
    }
}
