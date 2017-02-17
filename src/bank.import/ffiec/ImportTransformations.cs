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
    public class ImportTransformations
    {
        private static TaskPool<OrganizationFfiecTransformation> _taskPool = new TaskPool<OrganizationFfiecTransformation>();

        public static void Start(int threads)
        {
            InitializeTaskPool(threads);

            string path = @"c:\data\orgs\20161231_TRANSFORMATIONS.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(TransformationCollection));

            StreamReader reader = new StreamReader(path);
            var orgs = (TransformationCollection)serializer.Deserialize(reader);
            reader.Close();

            Console.WriteLine("Enqueuing {0} records", orgs.Transformations.Length);

            _taskPool.Enqueue(orgs.Transformations.ToList());
        }

        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting ffiec transformations import pool");
            _taskPool.MaxWorkers = threads;
            _taskPool.NextTask += _taskPool_NextTask; ;
        }

        private static void _taskPool_NextTask(OrganizationFfiecTransformation task)
        {
            Console.WriteLine("Processing {0} -> {1}", task.ID_RSSD_PREDECESSOR, task.ID_RSSD_SUCCESSOR);

            var orgRepo = new OrganizationRepository();
            var ffiecRepo = new OrganizationFfiecTransformationRepository();

            var existing = ffiecRepo.Get(task);


            if (existing != null)
            {
                return;
            }
            else
            {
                var predecessor = orgRepo.LookupByRssd(task.ID_RSSD_PREDECESSOR.Value);
                var successor = orgRepo.LookupByRssd(task.ID_RSSD_SUCCESSOR.Value);

                task.PredecessorOrganizationId = predecessor?.OrganizationId;

                ffiecRepo.Insert(task);
            }

            Console.WriteLine("Transformation {0} -> {1}", task.ID_RSSD_PREDECESSOR, task.ID_RSSD_SUCCESSOR);
        }

    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("DATA")]
    public class TransformationCollection
    {
        //[XmlArray("DATA")]
        //[XmlArrayItem("ATTRIBUTES", typeof(Organization))]
        [XmlElement("TRANSFORMATION")]
        public OrganizationFfiecTransformation[] Transformations { get; set; }
    }
}
