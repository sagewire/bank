using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using JeffFerguson.Gepsio.Xml.Interfaces;
using JeffFerguson.Gepsio.Xlink;

namespace JeffFerguson.Gepsio
{
    public class Reference : XlinkNode
    {
        public Reference(INode node) : base(node)
        {
            foreach(INode child in node.ChildNodes)
            {
                switch(child.LocalName)
                {
                    case "line":
                        Line = child.InnerText;
                        break;
                    case "column":
                        Column = child.InnerText;
                        break;
                }
            }
        }

        public string Line { get; set; }
        public string Column { get; set; }
    }
}
