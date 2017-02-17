using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using bank.data.repositories;
using bank.poco;

namespace bank.import.ffiec
{
    public class ImportOrganizations
    {
        private static TaskPool<OrganizationFfiec> _taskPool = new TaskPool<OrganizationFfiec>();

        public static void Start()
        {
            InitializeTaskPool();
            
            string active = @"c:\data\orgs\20161231_ATTRIBUTES_ACTIVE.xml";
            string inactive = @"c:\data\orgs\20161231_ATTRIBUTES_INACTIVE.xml";
            string branch = @"c:\data\orgs\20161231_ATTRIBUTES_BRANCH.xml";

            Enqueue(active);
            Enqueue(inactive);
            Enqueue(branch);
            
        }

        static void Enqueue(string path)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(OrganizationCollection));

            StreamReader reader = new StreamReader(path);
            var orgs = (OrganizationCollection)serializer.Deserialize(reader);
            reader.Close();

            Console.WriteLine("Enqueuing {0} records", orgs.Organization.Length);

            _taskPool.Enqueue(orgs.Organization.ToList());
        }

        static void InitializeTaskPool()
        {
            Console.WriteLine("Starting ffiec org import pool");
            _taskPool.MaxWorkers = 8;
            //_taskPool.MinimumQueueSize = 50;
            _taskPool.RefillQueue += _taskPool_RefillQueue;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        private static void _taskPool_NextTask(OrganizationFfiec task)
        {
            Console.WriteLine("Processing {0}", task.NM_LGL);

            var orgRepo = new OrganizationRepository();
            var ffiecRepo = Repository<OrganizationFfiec>.New();
            var ffiec = task;

            var existing = orgRepo.LookupByRssd(ffiec.ID_RSSD.Value);

            if (existing != null)
            {
                ffiec.OrganizationId = existing.OrganizationId;
                ffiecRepo.Save(ffiec);

                existing.ShortName = ffiec.NM_SHORT;
                orgRepo.Update(existing);
            }
            else
            {
                var org = new Organization
                {
                    Name = ffiec.NM_LGL,
                    ID_RSSD = ffiec.ID_RSSD.Value,
                    Created = DateTime.Now
                };

                orgRepo.Insert(org);
                ffiec.OrganizationId = org.OrganizationId;
                ffiecRepo.Insert(ffiec);
            }

            Console.WriteLine("Finished {0}", task.NM_LGL);

        }

        private static void _taskPool_QueueEmpty()
        {
            //throw new NotImplementedException();
            Console.WriteLine("org ffiec pool empty");
        }

        private static int _taskPool_RefillQueue(int queueCount)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("DATA")]
    public class OrganizationCollection
    {
        //[XmlArray("DATA")]
        //[XmlArrayItem("ATTRIBUTES", typeof(Organization))]
        [XmlElement("ATTRIBUTES")]
        public OrganizationFfiec[] Organization { get; set; }
    }
}
