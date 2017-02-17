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
                var definitions = conn.Query<ConceptDefinition>("select * from ConceptDefinition where ItemNumber in @names", new
                {
                    names = names.Select(x=>x.SafeSubstring(4,4)).ToList()
                },
                commandType: CommandType.Text)
                .ToList();

                return definitions;
            }
        }

    }
}
