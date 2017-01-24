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
    static class IndexOrganization
    {
        public static void Start()
        {
            var orgs = Repository<Organization>.New().All();

            foreach (var org in orgs)
            {
                Console.WriteLine(org.Name);
                Database.Index(org, org.OrganizationId.ToString(), "banks", "organization");
            }

        }
    }
}
