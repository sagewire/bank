using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Globalization;
using bank.data.repositories;
using bank.extensions;
using bank.poco;

namespace bank.import.ffiec
{
    public class ImportOrganizations
    {
        private static TaskPool<OrganizationFfiec> _taskPool = new TaskPool<OrganizationFfiec>();

        public static void Start(int threads)
        {
            //https://www.ffiec.gov/nicpubweb/nicweb/DataDownload.aspx

            InitializeTaskPool(threads);

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

        static void InitializeTaskPool(int threads)
        {
            Console.WriteLine("Starting ffiec org import pool");
            _taskPool.MaxWorkers = threads;
            //_taskPool.MinimumQueueSize = 50;
            _taskPool.RefillQueue += _taskPool_RefillQueue;
            _taskPool.QueueEmpty += _taskPool_QueueEmpty;
            _taskPool.NextTask += _taskPool_NextTask;
        }

        private static void _taskPool_NextTask(OrganizationFfiec task)
        {
            //Console.WriteLine("Processing {0}", task.NM_LGL);

            var orgRepo = new OrganizationRepository();
            var ffiecRepo = Repository<OrganizationFfiec>.New();
            var ffiec = task;

            var existing = orgRepo.LookupByRssd(ffiec.ID_RSSD.Value);

            if (existing != null)
            {
                ffiec.OrganizationId = existing.OrganizationId;
                ffiecRepo.Save(ffiec);

                existing.Name = existing.Name.ToTitleCase(true);
                existing.City = existing.City?.ToTitleCase(true) ?? ffiec.CITY.SafeSubstring(50).ToTitleCase(true);
                existing.State = existing.State ?? ffiec.STATE_ABBR_NM.SafeSubstring(2);
                existing.ZIP = existing.ZIP ?? ffiec.ZIP_CD.SafeSubstring(11);
                existing.Street1 = existing.Street1?.ToTitleCase(true) ?? ffiec.STREET_LINE1.SafeSubstring(80).ToTitleCase(true);
                existing.Street2 = existing.Street2?.ToTitleCase(true) ?? ffiec.STREET_LINE2.SafeSubstring(20).ToTitleCase(true);
                existing.Url = existing.Url ?? ffiec.URL.SafeSubstring(100);
                existing.EntityType = existing.EntityType ?? ffiec.ENTITY_TYPE;
                existing.Active = ffiec.REASON_TERM_CD == 0;
                existing.StatusCode = ffiec.REASON_TERM_CD;
                existing.ShortName = ffiec.NM_SHORT;
                orgRepo.Update(existing);
            }
            else
            {
                var org = new Organization
                {
                    Name = ffiec.NM_LGL.SafeSubstring(100).ToTitleCase(true),
                    City = ffiec.CITY.SafeSubstring(50).ToTitleCase(true),
                    State = ffiec.STATE_ABBR_NM,
                    Street1 = ffiec.STREET_LINE1.SafeSubstring(80).ToTitleCase(true),
                    Street2 = ffiec.STREET_LINE2.SafeSubstring(20).ToTitleCase(true),
                    ID_RSSD = ffiec.ID_RSSD.Value,
                    EntityType = ffiec.ENTITY_TYPE,
                    Url = ffiec.URL.SafeSubstring(100),
                    Active = ffiec.REASON_TERM_CD == 0,
                    StatusCode = ffiec.REASON_TERM_CD,
                    Created = DateTime.Now
                };

                orgRepo.Insert(org);
                ffiec.OrganizationId = org.OrganizationId;
                ffiecRepo.Insert(ffiec);
            }

            Console.WriteLine("Finished {0}", task.NM_LGL.ToTitleCase(true));

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
