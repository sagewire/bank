using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.crawl;
using bank.data.repositories;
using bank.poco;
using RestSharp.Extensions.MonoHttp;

namespace bank.import.crawl
{
    public static class Import
    {
        private static Facebook _facebook = new Facebook();
        private static Web _web = new Web();

        public static void Start(long? organizationId = null)
        {
            _facebook.TaskCompleted += _facebook_TaskCompleted;
            _facebook.Start();

            _web.TaskCompleted += _web_TaskCompleted;
            _web.Start();

            List<Organization> orgs;

            if (organizationId.HasValue)
            {
                orgs = new List<Organization> { Repository<Organization>.New().Get(organizationId.Value) };
            }
            else
            {
                orgs = Repository<Organization>.New().All()
                        .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                        //.Where(x => x.VerifiedUrl == null)
                        .ToList();
            }

            foreach (var org in orgs)
            {
                Console.WriteLine("{0}\t{1}", org.OrganizationId, org.Url);
                
                if (org.Url != null)
                {
                    var job = new WebJob { Organization = org, Url = org.Url };
                    _web.Enqueue(job);
                }
            }
        }

        private static void _web_TaskCompleted(WebJob job)
        {

            Repository<Organization>.New().Update(job.Organization);

            if (!string.IsNullOrWhiteSpace(job.Organization.Facebook))
            {
                _facebook.Enqueue(job.Organization);
            }
        }

        private static void _facebook_TaskCompleted(Organization org)
        {
            Repository<Organization>.New().Update(org);

            if (!string.IsNullOrWhiteSpace(org.Twitter) && 
                (string.IsNullOrWhiteSpace(org.Avatar) || 
                string.IsNullOrWhiteSpace(org.ProfileBanner)))
            {
                //need to check twitter for images
            }
        }

    }
}
