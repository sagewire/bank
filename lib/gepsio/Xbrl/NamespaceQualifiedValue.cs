
using JeffFerguson.Gepsio.Xml.Interfaces;
namespace JeffFerguson.Gepsio
{
    internal class NamespaceQualifiedValue
    {
        private string thisFullyQualifiedValue;
        private string[] thisFullyQualifiedValueComponents;
        private string thisLocalName;
        private string thisNamespace;
        private string thisNamespaceUri;

        internal bool HasNamespace
        {
            get
            {
                if (thisNamespace.Length == 0)
                    return false;
                return true;
            }
        }

        internal string LocalName
        {
            get
            {
                return thisLocalName;
            }
        }

        internal string Namespace
        {
            get
            {
                return thisNamespace;
            }
        }

        internal string NamespaceUri
        {
            get
            {
                return thisNamespaceUri;
            }
        }

        internal NamespaceQualifiedValue(INamespaceManager NamespaceManager, string FullyQualifiedValue)
        {
            thisFullyQualifiedValue = FullyQualifiedValue;
            thisFullyQualifiedValueComponents = thisFullyQualifiedValue.Split(':');
            if (thisFullyQualifiedValueComponents.Length == 1)
            {
                thisLocalName = thisFullyQualifiedValueComponents[0];
                thisNamespace = string.Empty;
                thisNamespaceUri = string.Empty;
            }
            else
            {
                thisLocalName = thisFullyQualifiedValueComponents[1];
                thisNamespace = thisFullyQualifiedValueComponents[0];
                thisNamespaceUri = NamespaceManager.LookupNamespace(thisNamespace);
            }
        }

        internal bool Equals(string NamespaceUri, string LocalName)
        {
            if (thisNamespaceUri.ToLower().Equals(NamespaceUri.ToLower()) == false)
                return false;
            if (thisLocalName.ToLower().Equals(LocalName.ToLower()) == false)
                return false;
            return true;
        }
    }
}
