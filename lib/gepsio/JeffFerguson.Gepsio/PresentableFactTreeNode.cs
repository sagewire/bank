using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JeffFerguson.Gepsio
{
    [DebuggerDisplay("{NodeLabel}")]
    /// <summary>
    /// A node in a tree of facts arranged in a parent/child relationship
    /// according to a presentation linkbase.
    /// </summary>
    public class PresentableFactTreeNode
    {
        /// <summary>
        /// The node's parent. This property is null if this node
        /// has no parent.
        /// </summary>
        public PresentableFactTreeNode ParentNode
        {
            get;
            private set;
        }

        public double Order { get; set; }

        /// <summary>
        /// The <see cref="Fact"/> object represented by this node.
        /// </summary>
        /// <remarks>
        /// This value may be null if the presentation linkbase arc for this tree
        /// node referenced a schema element marked as abstract. The tree node's
        /// <see cref="Label"/> property may have displayable information in leiu
        /// of an actual fact value.
        /// </remarks>
        public Fact NodeFact
        {
            get;
            internal set;
        }

        public bool IsAbstract { get; set; }

        /// <summary>
        /// The label for this node, as described by the label linkbase.
        /// </summary>
        /// <remarks>
        /// This information is automatically retrieved from the label linkbase,
        /// if it exists. If a label cannot be found, then this value is the empty string.
        /// This property is always available, regardless of whether or not the NodeFact
        /// is populated.
        /// </remarks>
        public string NodeLabel
        {
            get;
            internal set;
        }

        private List<PresentableFactTreeNode> _childNodes = new List<PresentableFactTreeNode>();
        /// <summary>
        /// A list of child nodes for this node. This collection may be
        /// empty if this node has no children.
        /// </summary>
        public List<PresentableFactTreeNode> ChildNodes
        {
            get
            {
                return _childNodes;
            }
            set
            {
                _childNodes = value;
            }
        }

        internal Locator PresentationLinkbaseLocator
        {
            get;
            set;
        }
        public string Name { get; set; }

        internal PresentableFactTreeNode()
        {
            ParentNode = null;
            NodeFact = null;
            //ChildNodes = new List<PresentableFactTreeNode>();
        }
    }
}
