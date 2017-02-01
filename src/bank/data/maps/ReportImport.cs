using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class ReportImportMapper : ClassMapper<ReportImport>
    {
        public ReportImportMapper()
        {
            Map(x => x.OrganizationId).Key(KeyType.NotAKey);
            Map(x => x.ReportType).Ignore();
            Map(x => x.ReportTypeAsString).Column("ReportType");
            Map(x => x.Created).ReadOnly();
            AutoMap();
        }
    }
}
