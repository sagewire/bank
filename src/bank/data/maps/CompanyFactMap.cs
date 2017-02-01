﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using DapperExtensions.Mapper;

namespace bank.data.maps
{
    internal class CompanyFactMapper : ClassMapper<CompanyFact>
    {
        public CompanyFactMapper()
        {
            Map(x => x.OrganizationId).Key(KeyType.NotAKey);
            AutoMap();
        }
    }
}