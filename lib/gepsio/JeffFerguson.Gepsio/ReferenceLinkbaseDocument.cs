using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Collections.Generic;

namespace JeffFerguson.Gepsio
{
    
    public class ReferenceLinkbaseDocument : LinkbaseDocument
    {
        
        public List<ReferenceLink> ReferenceLinks
        {
            get;
            private set;
        }

        internal ReferenceLinkbaseDocument(XbrlSchema ContainingXbrlSchema, string DocumentPath)
            : base(ContainingXbrlSchema, DocumentPath)
        {
            ReferenceLinks = new List<ReferenceLink>();
            foreach (INode CurrentChild in thisLinkbaseNode.ChildNodes)
            {
                if (CurrentChild.LocalName.Equals("referenceLink") == true)
                    this.ReferenceLinks.Add(new ReferenceLink(CurrentChild));
            }
        }
    }
}
