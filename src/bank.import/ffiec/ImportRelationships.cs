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

        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            string path = @"c:\data\orgs\20161231_RELATIONSHIPS.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(RelationshipCollection));

            StreamReader reader = new StreamReader(path);
            var orgs = (RelationshipCollection)serializer.Deserialize(reader);
            reader.Close();

            Console.WriteLine("Enqueuing {0} records", orgs.Relationships.Length);

            _taskPool.Enqueue(orgs.Relationships.ToList());
        }

        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting ffiec relationships import pool");
            _taskPool.MaxWorkers = threads;
            _taskPool.NextTask += _taskPool_NextTask; ;
        }

        private static void _taskPool_NextTask(OrganizationFfiecRelationship task)
        {
            Console.WriteLine("Processing {0} -> {1}", task.ID_RSSD_OFFSPRING, task.ID_RSSD_PARENT);

            var orgRepo = new OrganizationRepository();
            var ffiecRepo = new OrganizationFfiecRelationshipRepository();

            task.ParentOrganizationId = orgRepo.LookupByRssd(task.ID_RSSD_PARENT.Value)?.OrganizationId;
            task.OffspringOrganizationId = orgRepo.LookupByRssd(task.ID_RSSD_PARENT.Value)?.OrganizationId;

            var existing = ffiecRepo.Get(task);
            
            ffiecRepo.Save(task);
            
            
            Console.WriteLine("Relationship {0} -> {1}", task.ID_RSSD_PARENT, task.ID_RSSD_OFFSPRING);
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
