using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.data.elasticsearch;
using bank.data.repositories;
using bank.poco;

namespace bank.import.index
{
    public static class IndexOrganization
    {
        private static TaskPool<Organization> _taskPool = new TaskPool<Organization>();

        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting ffiec relationships import pool");
            _taskPool.MaxWorkers = threads;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        private static void _taskPool_NextTask(Organization org)
        {
            Console.WriteLine(org.Name);
            Database.Index(org, org.OrganizationId.ToString(), "banks", "organization");
        }
        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            var orgs = Repository<Organization>.New().All();

            _taskPool.Enqueue(orgs.ToList());
            
        }
    }
}
