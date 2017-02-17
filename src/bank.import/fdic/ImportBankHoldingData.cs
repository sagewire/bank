using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;
using CsvHelper;

namespace bank.import.fdic
{
    public class ImportBankHoldingData
    {
        private static TaskPool<Job> _taskPool = new TaskPool<Job>();
        private static Queue<string> _files = new Queue<string>();
        internal class Job
        {
            public string[] Fields { get; set; }
            public string[] Record { get; set; }
        }

        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            var files = Directory.GetFiles(@"c:\data\y9\", "*.txt");
            
            foreach (var file in files)
            {
                _files.Enqueue(file);
            }

            Enqueue(_files.Dequeue());
        }

        private static void Enqueue(string filename)
        {


            var reader = new StreamReader(filename);

            var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = "^";
            csv.ReadHeader();

            while (csv.Read())
            {
                _taskPool.Enqueue(new Job { Fields = csv.FieldHeaders, Record = csv.CurrentRecord });

            }
        }

        static void InitializeTaskPool(int threads)
        {
            _taskPool.MaxWorkers = threads;

            _taskPool.NextTask += _taskPool_NextTask;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty;
            _taskPool.RefillQueue += _taskPool_RefillQueue;
        }

        private static int _taskPool_RefillQueue(int queueCount)
        {
            if (_files.Count > 0)
            {
                var file = _files.Dequeue();

                Enqueue(file);

                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static void _taskPool_QueueEmpty()
        {
            Console.WriteLine("Empty");
        }

        private static void _taskPool_NextTask(Job task)
        {
            int? id_rssd = null;
            DateTime? reportingDate = null;

            var facts = new Dictionary<string, string>();

            var index = 0;
            foreach (var header in task.Fields)
            {
                var datum = task.Record[index++].Trim();

                if (datum.StartsWith("---")) return;

                switch (header)
                {
                    case "RSSD9001":
                        id_rssd = int.Parse(datum);
                        break;
                    case "RSSD9999":
                        reportingDate = DateTime.ParseExact(datum, "yyyyMMdd", null);
                        break;
                }

                if (!string.IsNullOrWhiteSpace(datum) && datum != "0")
                {
                    facts.Add(header.Trim(), datum.Trim());
                }
            }

            Console.WriteLine("Processing {0}", id_rssd);

            var orgRepo = new OrganizationRepository();
            var org = orgRepo.LookupByRssd(id_rssd.Value);

            if (org == null)
            {
                Console.WriteLine("No Org found for {0}", id_rssd);
                return;
            }

            var factRepo = new FactRepository();

            foreach (var item in facts)
            {
                if (item.Key.Length > 8) continue;

                var factObj = new CompanyFact
                {
                    OrganizationId = org.OrganizationId,
                    Name = item.Key,
                    Period = reportingDate.Value

                };

                factObj.SetValue(item.Value);

                factRepo.Save(factObj);
            }

        }
    }
}
