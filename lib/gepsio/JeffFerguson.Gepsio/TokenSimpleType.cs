using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace JeffFerguson.Gepsio
{
    public class TokenSimpleType : RestrictedSimpleType
    {
        private List<string> thisEnumerationValues;

        internal TokenSimpleType(XmlNode SimpleTypeNode, XmlNode RestrictionNode)
            : base(SimpleTypeNode, RestrictionNode)
        {
            thisEnumerationValues = new List<string>();
            foreach (XmlNode CurrentChildNode in RestrictionNode.ChildNodes)
            {
                if (CurrentChildNode.LocalName.Equals("enumeration") == true)
                    thisEnumerationValues.Add(CurrentChildNode.Attributes["value"].Value);
            }
        }
    }
}
