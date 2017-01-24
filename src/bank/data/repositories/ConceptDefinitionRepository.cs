using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.extensions;
using bank.poco;
using Dapper;

namespace bank.data.repositories
{
    public class ConceptDefinitionRepository : BaseRepository<ConceptDefinition>
    {
        public List<ConceptDefinition> Filter(IList<string> names)
        {
            using (var conn = new SqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                var definitions = conn.Query<ConceptDefinition>("select * from ConceptDefinition where Mdrm in @names", new
                {
                    names = names
                },
                commandType: CommandType.Text)
                .ToList();

                return definitions;
            }
        }

    }
}
