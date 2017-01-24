using JeffFerguson.Gepsio.Xsd;
using System.Collections.Generic;
using System.Linq;
using System;

namespace JeffFerguson.Gepsio
{
    /// <summary>
    /// A tree of facts arranged in a parent/child relationship
    /// according to a presentation linkbase.
    /// </summary>
    public class PresentableFactTree
    {
        /// <summary>
        /// A collection of top-level nodes in the tree.
        /// </summary>
        /// <remarks>
        /// This property is a list of top-level presentable tree nodes,
        /// each of which is represented by a class called
        /// <see cref="PresentableFactTreeNode"/>. Unlike traditional trees
        /// in computer science, which have a single "root" node at the top,
        /// presentation linkbases do not necessarily define a single "root"
        /// top level node. Gepsio, therefore, must prepare for the scenario
        /// in which the presentation linkbase has multiple "top-level" nodes
        /// and defines a list of top level nodes in a presentable fact tree.
        /// </remarks>
        public List<PresentableFactTreeNode> TopLevelNodes
        {
            get;
            private set;
        }

        private PresentableFactTreeNode CreateNode(XbrlSchema schema, Locator locator, FactCollection facts)
        {

            var element = GetElement(schema, locator.HrefResourceId);

            var node = new PresentableFactTreeNode
            {
                NodeLabel = GetLabel(schema, locator),
                PresentationLinkbaseLocator = locator,
                Name = element.Name,
                NodeFact = facts.GetFactByName(element.Name),
                IsAbstract = element.IsAbstract

            };
            
            
            return node;
        }

        internal PresentableFactTree(XbrlSchema schema, FactCollection facts)
        {
            TopLevelNodes = new List<PresentableFactTreeNode>();
            var presentationLinkbase = schema.PresentationLinkbase;
            foreach (var presentationLink in presentationLinkbase.PresentationLinks)
            {
                var unorderedPresentationArcs = presentationLink.PresentationArcs;
                var orderedPresentationArcs = unorderedPresentationArcs.ToList();//MP .OrderBy(o => o.Order).ToList();
                var newTreeNode = new PresentableFactTreeNode();
                //TopLevelNodes.Add(newTreeNode);

                var lookup = new Dictionary<string, PresentableFactTreeNode>();
                var hasRoot = false;

                foreach (var orderedPresentationArc in orderedPresentationArcs)
                {

                    var fromLocator = GetLocator(orderedPresentationArc.From, presentationLink);
                    var toLocator = GetLocator(orderedPresentationArc.To, presentationLink);

                    PresentableFactTreeNode fromTreenode;
                    if (!lookup.ContainsKey(fromLocator.Href))
                    {
                        fromTreenode = CreateNode(schema, fromLocator, facts);
                        lookup.Add(fromLocator.Href, fromTreenode);

                        if (!hasRoot)
                        {
                            hasRoot = true;
                            TopLevelNodes.Add(fromTreenode);
                        }
                    }
                    else
                    {
                        fromTreenode = lookup[fromLocator.Href];
                    }

                    PresentableFactTreeNode toTreenode;
                    if (!lookup.ContainsKey(toLocator.Href))
                    {
                        toTreenode = CreateNode(schema, toLocator, facts);
                        toTreenode.Order = orderedPresentationArc.Order;

                        lookup.Add(toLocator.Href, toTreenode);
                    }
                    else
                    {
                        toTreenode = lookup[toLocator.Href];
                    }

                    fromTreenode.ChildNodes.Add(toTreenode);

                    //if (!toTreenode.IsAbstract)
                    //{
                    //    foreach(var referenceLink in schema.ReferenceLinkbase.ReferenceLinks)
                    //    {
                    //        try {
                    //            var loc = referenceLink.Locators.SingleOrDefault(x => x.HrefResourceId == toTreenode.PresentationLinkbaseLocator.HrefResourceId);

                    //            var refArc = referenceLink.ReferenceArcs.SingleOrDefault(x => x.From == loc.Label);

                    //            var reference = referenceLink.References.SingleOrDefault(x => x.Label == refArc.To);
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Console.WriteLine("");
                    //        }
                    //    }
                    //}

                    fromTreenode.ChildNodes.Sort((a, b) => a.Order.CompareTo(b.Order));

                }
            }
        }

        /// <summary>
        /// Returns the label stored in a label linkbase for a given locator.
        /// </summary>
        /// <param name="schema">
        /// The compiled schema which whose elements should be searched.
        /// </param>
        /// <param name="labelLocator">
        /// The locator of the label to be returned.
        /// </param>
        /// <returns>
        /// The label for the given locator, or an empty string if there is no 
        /// label.
        /// </returns>
        private string GetLabel(XbrlSchema schema, Locator labelLocator)
        {
            //MP
            foreach (var labelLinkbase in schema.LabelLinkbase)
            {
                //var labelLinkbase = schema.LabelLinkbase;
                //if (labelLinkbase == null)
                //  return string.Empty;
                foreach (var labelLink in labelLinkbase.LabelLinks)
                {
                    var foundLabel = GetLabel(schema, labelLocator, labelLink);
                    if (string.IsNullOrEmpty(foundLabel) == false)
                        return foundLabel;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the label stored in a label linkbase for a given locator.
        /// </summary>
        /// <param name="schema">
        /// The compiled schema which whose elements should be searched.
        /// </param>
        /// <param name="labelLocator">
        /// The locator of the label to be returned.
        /// </param>
        /// <param name="labelLink">
        /// The link label to be searched.
        /// </param>
        /// <returns>
        /// The label for the given locator, or an empty string if there is no 
        /// label.
        /// </returns>
        private string GetLabel(XbrlSchema schema, Locator labelLocator, LabelLink labelLink)
        {
            var labelLinkbase = schema.LabelLinkbase;
            if (labelLinkbase == null)
                return string.Empty;

            // Find label locator with href matching labelLocator.Href
            // Find label arc with from matching label locator.Label
            // find label with label matching label arc.To
            // grab innerText ... GET HYPE

            var labelLinkLocator = labelLink.GetLocator(labelLocator.Href);
            if (labelLinkLocator == null)
                return string.Empty;
            var labelArc = labelLink.GetLabelArc(labelLinkLocator.Label);
            if (labelArc == null)
                return string.Empty;
            var finalLabel = labelLink.GetLabel(labelArc.ToId);
            if (finalLabel == null)
                return string.Empty;
            return finalLabel.Text;
        }

        /// <summary>
        /// Finds an element in the given schema with the given ID.
        /// </summary>
        /// <param name="schema">
        /// The compiled schema which whose elements should be searched.
        /// </param>
        /// <param name="id">
        /// The ID of the element to return.
        /// </param>
        /// <returns>
        /// The element whose ID matches the given ID, or null if no element can be found.
        /// </returns>
        private Element GetElement(XbrlSchema schema, string id)
        {
            foreach (var candidateElement in schema.Elements)
            {
                if (string.IsNullOrEmpty(candidateElement.Id) == false)
                {
                    if (candidateElement.Id.Equals(id) == true)
                        return candidateElement;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a locator in the given presentation link with the given name.
        /// </summary>
        /// <param name="name">
        /// The name of the locator to return.
        /// </param>
        /// <param name="presentationLink">
        /// The presentation link whose locators should be searched.
        /// </param>
        /// <returns>
        /// The locator whose label matches the given name, or null if no locator can be found.
        /// </returns>
        private Locator GetLocator(string name, PresentationLink presentationLink)
        {
            foreach (var candidateLocator in presentationLink.Locators)
            {
                if (candidateLocator.Label.Equals(name) == true)
                    return candidateLocator;
            }
            return null;
        }
    }
}
