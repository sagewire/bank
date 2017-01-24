using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions;

namespace bank.data.repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        public virtual void Save(T model)
        {
            throw new NotSupportedException();
        }

        public virtual T Get(T model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                var result = conn.Get<T>(model);

                conn.Close();

                return result;
            }
        }

        public virtual T Get(long id)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                var result = conn.Get<T>(id);

                conn.Close();

                return result;
            }
        }

        public virtual T Get(string id)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                var result = conn.Get<T>(id);

                conn.Close();

                return result;
            }
        }

        public virtual IList<T> All()
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var predicate = Predicates.Field<Organization>(x => x.OrganizationId, Operator.Gt, 0);

                var result = conn.GetList<T>(predicate);

                conn.Close();

                return result.ToList();
            }
        }



        public virtual void Insert(T model)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();

                conn.Insert(model);

                conn.Close();
            }
        }

        public virtual void Update(T model)
        {
            try {
                using (var conn = new SqlConnection(Settings.ConnectionString))
                {
                    conn.Open();

                    conn.Update<T>(model);

                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}
