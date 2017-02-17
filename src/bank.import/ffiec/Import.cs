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
using System.Threading;

namespace bank.import.ffiec
{
    public class Import
    {
        private static Regex _id = new Regex(@"(?<=\s)\d+?(?=\(ID RSSD\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _version = new Regex(@"(?<=ubpr-)v\d{2,3}(?=-Core)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static TaskPool<ReportImport> _taskPool = new TaskPool<ReportImport>();

        public static void Start()
        {
            Console.WriteLine("starting import");

            InitializeTaskPool();

            var doc = new XbrlDocument();

            //var files = Directory.GetFiles("c:/data/ubpr-data", "ffiec cdr ubpr*");
            //var files = Directory.GetFiles("c:/data/ubpr-data", "error ffiec cdr ubpr*");
            //var files = Directory.GetFiles("c:/data/call-data/09302016_Form041", "ffiec cdr call*");
            //var files = Directory.GetFiles("c:/data/ubpr-data", "cdr-ubpr*");


            _taskPool.Start();


 

        }

        static void InitializeTaskPool()
        {
            _taskPool.MaxWorkers = 12;
            _taskPool.MinimumQueueSize = 50;

            _taskPool.RefillQueue += _taskPool_RefillQueue;
            _taskPool.NextTask += _taskPool_NextTask;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty; ;

        }

        private static int _taskPool_RefillQueue(int queueCount)
        {

            var repo = new ReportImportRepository();
            var records = repo.GetRecordsToImport(100);

            foreach (var record in records)
            {
                //Console.Write(".");
                Console.WriteLine("Enqueuing {0}\t{1}\t{2}", record.Period, record.OrganizationId, record.Filed);
                _taskPool.Enqueue(record);
            }

            return records.Count;
        }

        private static void _taskPool_NextTask(ReportImport task)
        {
            Console.WriteLine("Next Import Task {0} {1} {2}", task.Period, task.OrganizationId, task.ReportType);
            ProcessFile(task);
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
                    ID_RSSD = id
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

        static string RetrieveFile(ReportImport reportImport, Organization org)
        {
            var userToken = new UsernameToken(Settings.FfiecWebServiceUsername, Settings.FfiecWebServiceToken, PasswordOption.SendHashed);
            var ffiec = new gov.ffiec.cdr.RetrievalService();

            ffiec.RequestSoapContext.Security.Tokens.Add(userToken);

            if (reportImport.ReportType == ReportTypes.Call)
            {
                var result = ffiec.RetrieveFacsimile(
                    gov.ffiec.cdr.ReportingDataSeriesName.Call,
                    reportImport.Period.ToShortDateString(), 
                    gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD,
                    org.ID_RSSD,
                    gov.ffiec.cdr.FacsimileFormat.XBRL);

                return null;
            }
            else
            {
                var result = ffiec.RetrieveUBPRXBRLFacsimile(
                    reportImport.Period.ToShortDateString(),
                    gov.ffiec.cdr.FinancialInstitutionIDType.ID_RSSD,
                    org.ID_RSSD);

                var text = UTF8Encoding.UTF8.GetString(result);

                var file = Path.Combine(
                    Settings.ReportSchemas, 
                    reportImport.ReportTypeAsString.ToLower(), 
                    _version.Match(text).Value, 
                    reportImport.OrganizationId.ToString() + ".xml");

                File.WriteAllBytes(file, result);

                return file; 
            }
        }

        static void ProcessFile(ReportImport reportImport)
        {
            var org = Repository<Organization>.New().Get(reportImport.OrganizationId);

            var file = RetrieveFile(reportImport, org);

            
            //File.WriteAllBytes(
            //    string.Format(@"c:\temp\downloads\{0}-{1}-{2}.xml", org.OrganizationId, reportImport.ReportTypeAsString, reportImport.Period.ToString("yyyyMMdd")),
            //    fileContents);

            //return;
            
            var moveFolder = "completed";

            XbrlDocument doc = new XbrlDocument();
            Item lastFact;
            try
            {
                doc.Load(file);
                
                if (!doc.IsValid)
                {
                    //throw new Exception(string.Format("Invalid XBRL {0}", file));
                }

                foreach (Item fact in doc.XbrlFragments[0].Facts)//.Where(x=>x.Name == "RIAD4300"))
                {
                    lastFact = fact;

                    decimal numeric;
                    decimal? numericValue = null;
                    string value = fact.Value;
                    
                    DateTime? period = null;
                    if (fact.ContextRef.InstantDate != null && fact.ContextRef.InstantDate > DateTime.MinValue)
                    {
                        period = fact.ContextRef.InstantDate;
                    }
                    else
                    {
                        period = GetDate(fact.ContextRef.PeriodEndDate);
                    }

                    if (period != reportImport.Period)
                    {
                        continue;
                    }

                    if (decimal.TryParse(fact.Value, out numeric))
                    {
                        numericValue = numeric;
                        value = null;
                    }

                    var factObj = new bank.poco.CompanyFact
                    {
                        OrganizationId = org.OrganizationId,
                        Name = fact.Name,
                        NumericValue = numericValue,
                        Value = value,
                        Period = period
                    };

                    if (!string.IsNullOrWhiteSpace(fact.UnitRefName))
                    {
                        factObj.Unit = fact.UnitRefName.ToUpper().First();
                    }

                    if ((factObj.NumericValue.HasValue && factObj.NumericValue.Value > 0) || !string.IsNullOrWhiteSpace(factObj.Value))
                    {
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", System.Threading.Thread.CurrentThread.ManagedThreadId, fact.Id, fact.Name, fact.Value);

                        Repository<bank.poco.Fact>.New().Save(factObj);
                    }
                }

                reportImport.State = null;
                reportImport.Processed = DateTime.Now;
            }
            catch (Exception e)
            {
                doc = null;
                moveFolder = "error";
                reportImport.State = "error";
                Console.WriteLine(e);
            }

            var filename = Path.GetFileName(file);
            var movePath = Path.Combine(Path.GetDirectoryName(file), moveFolder);
            movePath = Path.Combine(movePath, filename);

            var reportImportRepo = new ReportImportRepository();
            reportImportRepo.Save(reportImport);

            File.Delete(file);

            //File.Move(file, movePath);
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
}
