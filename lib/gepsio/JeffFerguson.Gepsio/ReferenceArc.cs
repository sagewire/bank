using JeffFerguson.Gepsio.Xlink;
using JeffFerguson.Gepsio.Xml.Interfaces;
using System;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// An encapsulation of the XBRL element "presentationArc" as defined in the http://www.xbrl.org/2003/linkbase namespace.
    /// </summary>
    public class ReferenceArc : XlinkNode
    {
        /// <summary>
        /// The order of the presentation arc amongst all of the arcs in the same presentation link.
        /// </summary>
        /// <remarks>
        /// This value is populated from the value of the arc's "order" attribute.
        /// </remarks>
        

        internal ReferenceArc(INode presentationArcNode) : base(presentationArcNode)
        {
            
        }
    }
}
