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
    public class ImportRelationships
    {
        private static TaskPool<OrganizationFfiecRelationship> _taskPool = new TaskPool<OrganizationFfiecRelationship>();

        public static void Start()
        {
            InitializeTaskPool();

            string path = @"c:\temp\orgs\20161231_RELATIONSHIPS.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(RelationshipCollection));

            StreamReader reader = new StreamReader(path);
            var orgs = (RelationshipCollection)serializer.Deserialize(reader);
            reader.Close();

            Console.WriteLine("Enqueuing {0} records", orgs.Relationships.Length);

            _taskPool.Enqueue(orgs.Relationships.ToList());
        }

        static void InitializeTaskPool()
        {
            Console.WriteLine("Starting ffiec relationships import pool");
            _taskPool.MaxWorkers = 8;
            _taskPool.NextTask += _taskPool_NextTask; ;
        }

        private static void _taskPool_NextTask(OrganizationFfiecRelationship task)
        {
            Console.WriteLine("Processing {0} -> {1}", task.ID_RSSD_OFFSPRING, task.ID_RSSD_PARENT);

            var orgRepo = new OrganizationRepository();
            var ffiecRepo = Repository<OrganizationFfiecRelationship>.New();
            var ffiec = task;

            ffiecRepo.Insert(ffiec);

            //var existing = orgRepo.LookupByRssd(ffiec.ID_RSSD.Value);

            //if (existing != null)
            //{
            //    ffiec.OrganizationId = existing.OrganizationId;
            //    ffiecRepo.Save(ffiec);

            //    existing.ShortName = ffiec.NM_SHORT;
            //    orgRepo.Update(existing);
            //}
            //else
            //{
            //    var org = new Organization
            //    {
            //        Name = ffiec.NM_LGL,
            //        ID_RSSD = ffiec.ID_RSSD.Value,
            //        Created = DateTime.Now
            //    };

            //    orgRepo.Insert(org);
            //    ffiec.OrganizationId = org.OrganizationId;
            //    ffiecRepo.Insert(ffiec);
            //}

            //Console.WriteLine("Finished {0}", task.NM_LGL);
        }

    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("DATA")]
    public class RelationshipCollection
    {
        //[XmlArray("DATA")]
        //[XmlArrayItem("ATTRIBUTES", typeof(Organization))]
        [XmlElement("RELATIONSHIP")]
        public OrganizationFfiecRelationship[] Relationships { get; set; }
    }
}
