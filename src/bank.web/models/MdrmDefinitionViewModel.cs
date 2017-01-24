using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;

namespace bank.web.models
{
    public class MdrmDefinitionViewModel
    {
        public MdrmDefinition Definition { get; internal set; }
        public string Mdrm { get; set; }

        public ChartConfig Chart { get; set; }

    }
}