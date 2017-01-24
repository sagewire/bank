using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.data.repositories
{
    public interface IRepository<T> where T : class
    {
        void Save(T model);
        void Update(T model);
        void Insert(T model);
        T Get(long id);
        T Get(string id);
        T Get(T model);
        IList<T> All();
    }
}
