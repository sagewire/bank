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
        private static TaskPool<string[]> _taskPool = new TaskPool<string[]>();
        private static string[] _fields;

        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            var filename = @"C:\Data\y9\bhcf1603.txt";

            var reader = new StreamReader(filename);

            var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = "^";
            csv.ReadHeader();

            _fields = csv.FieldHeaders;

            while (csv.Read())
            {
                _taskPool.Enqueue(csv.CurrentRecord);

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
            return 0;
        }

        private static void _taskPool_QueueEmpty()
        {

        }

        private static void _taskPool_NextTask(string[] task)
        {
            int? id_rssd = null;
            DateTime? reportingDate = null;

            var facts = new Dictionary<string, string>();

            var index = 0;
            foreach (var header in _fields)
            {
                var datum = task[index++].Trim();

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
            var factRepo = new FactRepository();

            foreach(var item in facts)
            {
                if (item.Key.Length > 8) continue;

                var factObj = new CompanyFact
                {
                    OrganizationId =org.OrganizationId,
                    Name = item.Key,
                    Period = reportingDate.Value
                   
                };

                factObj.SetValue(item.Value);

                factRepo.Save(factObj);   
            }

        }
    }
}
