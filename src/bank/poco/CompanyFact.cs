﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.enums;

namespace bank.poco
{
    public class CompanyFact : Fact
    {
        public int OrganizationId { get; set; }

        public override FactTypes FactType
        {
            get
            {
                return FactTypes.Company;
            }
        }

    }
}