using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;

namespace bank.data.repositories
{
    public static class Repository<T> where T : class
    {
        public static IRepository<T> New()
        {
            switch(typeof(T).Name)
            {
                case "Fact":
                    var fact = new FactRepository();
                    return (IRepository<T>)fact;
                case "Organization":
                    var org = new OrganizationRepository();
                    return (IRepository<T>)org;
                case "TwitterFriend":
                    var friend = new TwitterFriendRepository();
                    return (IRepository<T>)friend;
                case "ReportImport":
                    return (IRepository<T>)new ReportImportRepository();
                case "MdrmDefinition":
                    return (IRepository<T>)new ConceptDefinitionRepository();
                default:
                    var baseRepo = new BaseRepository<T>();
                    return (IRepository<T>)baseRepo;
            }
        }
    }
}
