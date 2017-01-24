using JeffFerguson.Gepsio.Xlink;
using JeffFerguson.Gepsio.Xml.Interfaces;
using System.Collections.Generic;

namespace JeffFerguson.Gepsio
{
    
    public class ReferenceLink : XlinkNode
    {
        public List<Locator> Locators
        {
            get;
            private set;
        }

     
        public List<ReferenceArc> ReferenceArcs
        {
            get;
            private set;
        }

        public List<Reference> References { get; set; } = new List<Reference>();

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        internal ReferenceLink(INode referenceLinkNode) : base(referenceLinkNode)
        {
            Locators = new List<Locator>();
            Locators.Capacity = referenceLinkNode.ChildNodes.Count;
            ReferenceArcs = new List<ReferenceArc>();
            ReferenceArcs.Capacity = referenceLinkNode.ChildNodes.Count;
            foreach (INode CurrentChild in referenceLinkNode.ChildNodes)
            {
                switch (CurrentChild.LocalName.ToLower())
                {
                    case "loc":
                        Locators.Add(new Locator(CurrentChild));
                        break;
                    case "referencearc":
                        ReferenceArcs.Add(new ReferenceArc(CurrentChild));
                        break;
                    case "reference":
                        References.Add(new Reference(CurrentChild));
                        break;
                }
            }

            //MP
            //SortPresentationArcsInAscendingOrder();
            Locators.TrimExcess();
            ReferenceArcs.TrimExcess();
        }
        
    }
}
