using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.poco
{
    public class ConceptDefinition
    {
        public string Mdrm { get; set; }
        public string Section { get; set; }
        public string ItemNumber { get; set; }
        public string Title { get; set; }
        public string SubTitle{ get; set; }
        public string Description { get; set; }
        public string Narrative { get; set; }
        public string Formula { get; set; }
        public char? Unit { get; set; }
        public bool? Negative { get; set; }
        public string Balance { get; set; }
    }
}
