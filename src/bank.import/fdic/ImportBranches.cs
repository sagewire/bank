using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.repositories;
using bank.poco;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace bank.import.fdic
{
    public class ImportBranches
    {
        private static TaskPool<BranchFdic> _taskPool = new TaskPool<BranchFdic>();

        public static void Start()
        {
            InitializeTaskPool();

            //var filename = @"c:/temp/orgs/branches-sample.csv";
            //var filename = @"c:/temp/orgs/ALL_2016.csv";
            //var filename = @"c:/temp/orgs/ALL_2016_1.csv";
            //var filename = @"c:/temp/orgs/ALL_2016_2.csv";

            Enqueue(@"c:/temp/orgs/ALL_2016_1.csv");
            Enqueue(@"c:/temp/orgs/ALL_2016_2.csv");
            //}
        }

        static void Enqueue(string filename
            )
        {

            var reader = new StreamReader(filename);

            //TypeConverterOptionsFactory.GetOptions(typeof(int)).NumberStyle = NumberStyles.Any;


            var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = true;

            csv.Configuration.RegisterClassMap<BranchFdicMap>();
            //csv.Configuration.IgnoreQuotes = true;

            var records = csv.GetRecords<BranchFdic>().ToList();

            _taskPool.Enqueue(records);
        }

        static void InitializeTaskPool()
        {
            Console.WriteLine("Starting fdic branch import pool");
            _taskPool.MaxWorkers = 8;
            _taskPool.NextTask += _taskPool_NextTask; ;
        }

        private static void _taskPool_NextTask(BranchFdic task)
        {
            Console.WriteLine("Processing {0} -> {1}", task.RSSDID, task.UNINUMBR);

            var ffiecRepo = Repository<BranchFdic>.New();

            ffiecRepo.Insert(task);
        }
    }

    class BranchFdicMap : CsvClassMap<bank.poco.BranchFdic>
    {
        public BranchFdicMap()
        {
            //Map(x => x.UNINUMBR).Name("UNINUMBR");
            //Map(x => x.SIMS_ACQUIRED_DATE).Ignore();
            //Map(x => x.SIMS_ESTABLISHED_DATE).Ignore();
            AutoMap();

            Map(x => x.DEPSUMBR).Name("DEPSUMBR").TypeConverterOption(NumberStyles.Any);
            Map(x => x.ASSET).Name("ASSET").TypeConverterOption(NumberStyles.Any);
            Map(x => x.DEPDOM).Name("DEPDOM").TypeConverterOption(NumberStyles.Any);
            Map(x => x.DEPSUM).Name("DEPSUM").TypeConverterOption(NumberStyles.Any);
        }
    }

}
