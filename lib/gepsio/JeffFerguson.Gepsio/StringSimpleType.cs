using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace JeffFerguson.Gepsio
{
    public class StringSimpleType : RestrictedSimpleType
    {
        internal StringSimpleType(XmlNode SimpleTypeNode, XmlNode RestrictionNode)
            : base(SimpleTypeNode, RestrictionNode)
        {
        }
    }
}
