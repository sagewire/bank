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

namespace bank.import.ffiec
{
    class Import
    {
        private static Regex _id = new Regex(@"(?<=\s)\d+?(?=\(ID RSSD\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static TaskPool<string> _taskPool = new TaskPool<string>();

        public static void Start()
        {
            Console.WriteLine("starting import");

            InitializeTaskPool();

            var doc = new XbrlDocument();
            
            //var files = Directory.GetFiles("c:/data/ubpr-data", "ffiec cdr ubpr*");
            var files = Directory.GetFiles("c:/data/call-data/09302016_Form041", "ffiec cdr call*");
            //var files = Directory.GetFiles("c:/data/ubpr-data", "cdr-ubpr*");
            
            _taskPool.Start();


            foreach (var file in files)
            {
                Console.WriteLine("Enqueuing {0}", file);


                _taskPool.Enqueue(file);

            }

        }

        static void InitializeTaskPool()
        {
            _taskPool.MaxWorkers = 12;

            _taskPool.RefillQueue += _taskPool_RefillQueue;
            _taskPool.NextTask += _taskPool_NextTask;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty; ;

        }

        private static int _taskPool_RefillQueue(int queueCount)
        {
            return 0;
        }

        private static void _taskPool_NextTask(string task)
        {
            Console.WriteLine("Next Task {0}", task);
            ProcessFile(task);
        }

        private static void _taskPool_QueueEmpty()
        {
            Console.WriteLine("Queue empty");
        }

        static Organization GetOrg(int id)
        {
            var orgRepo = new OrganizationRepository();
            var org = orgRepo.LookupByFFIEC(id);

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

            for(int i = 0; i <= level; i++)
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

            foreach(var child in node.ChildNodes)
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

            foreach(var childnode in node.ChildNodes)
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

                    var factObj = new bank.poco.Fact
                    {
                        OrganizationId = org.OrganizationId,
                        Name = fact.Name,
                        NumericValue = numericValue,
                        Value = fact.Value,
                        Period = period
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
}
