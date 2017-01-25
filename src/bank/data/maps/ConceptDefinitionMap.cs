using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    public class ConceptDefinitionMap : ClassMapper<ConceptDefinition>
    {
        public ConceptDefinitionMap()
        {
            Map(x => x.Mdrm).Key(KeyType.Assigned);
            AutoMap();
        }
    }
}
