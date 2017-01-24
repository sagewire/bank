using System.Xml;
using System.Xml.Schema;

namespace JeffFerguson.Gepsio
{
    // Various extension methods for .NET Framework classes.

    internal static class ExtensionMethods
    {
        //====================================================================================
        #region XmlNode Extension Methods
        //====================================================================================

        //------------------------------------------------------------------------------------
        // Returns true if this XmlNode structure equals (s-equals) the supplied XmlNode and
        // returns false otherwise.
        //------------------------------------------------------------------------------------
		internal static bool StructureEquals(this XmlNode ThisNode, XmlNode OtherNode)
        {
            if (OtherNode == null)
                return false;
            if (ThisNode.NamespaceURI.Equals(OtherNode.NamespaceURI) == false)
                return false;
            if (ThisNode.LocalName.Equals(OtherNode.LocalName) == false)
                return false;
            return ThisNode.ChildNodes.StructureEquals(OtherNode.ChildNodes);
        }

        //------------------------------------------------------------------------------------
        // Returns true if this XmlNode parent equals (p-equals) the supplied XmlNode and
        // returns false otherwise.
        //------------------------------------------------------------------------------------
		internal static bool ParentEquals(this XmlNode ThisNode, XmlNode OtherNode)
        {
            if (OtherNode == null)
                return false;
            return object.ReferenceEquals(ThisNode.ParentNode, OtherNode.ParentNode);
        }

        //====================================================================================
        #endregion
        //====================================================================================

        //====================================================================================
        #region XmlNodeList Extension Methods
        //====================================================================================

        //------------------------------------------------------------------------------------
        // Returns true if this XmlNodeList structure equals (s-equals) the supplied
        // XmlNodeList and returns false otherwise.
        //------------------------------------------------------------------------------------
		internal static bool StructureEquals(this XmlNodeList ThisNodeList, XmlNodeList OtherNodeList)
        {
            if ((ThisNodeList == null) && (OtherNodeList != null))
                return false;
            if ((ThisNodeList != null) && (OtherNodeList == null))
                return false;
            if (ThisNodeList.Count != OtherNodeList.Count)
                return false;
            for (int NodeIndex = 0; NodeIndex < ThisNodeList.Count; NodeIndex++)
            {
                if (ThisNodeList[NodeIndex].StructureEquals(OtherNodeList[NodeIndex]) == false)
                    return false;
            }
            return true;
        }

        //====================================================================================
        #endregion
        //====================================================================================

        //====================================================================================
        #region XmlSchemaType Extension Methods
        //====================================================================================
		internal static bool IsNumeric(this XmlSchemaType ThisXmlSchemaType)
        {
            switch (ThisXmlSchemaType.TypeCode)
            {
                case XmlTypeCode.Decimal:
                case XmlTypeCode.Double:
                case XmlTypeCode.Float:
                case XmlTypeCode.Int:
                case XmlTypeCode.Integer:
                case XmlTypeCode.Long:
                case XmlTypeCode.NegativeInteger:
                case XmlTypeCode.NonNegativeInteger:
                case XmlTypeCode.NonPositiveInteger:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.Short:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.UnsignedShort:
                    return true;
                default:
                    return false;
            }
        }

        //====================================================================================
        #endregion
        //====================================================================================
    }
}
