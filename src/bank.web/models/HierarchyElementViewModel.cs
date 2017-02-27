using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.poco.graph;
using bank.reports;

namespace bank.web.models
{
    public class HierarchyElementViewModel
    {
        public decimal Limit { get; set; }
        public Column Column { get; set; }
        //public List<Concept> Concepts { get; set; }
        public HierarchyElement Element { get; set; }

        public List<object> Flatten()
        {
            var data = new List<object>();
            var relativeConcept = Element.AllConcepts.SingleOrDefault(x => x.Name == Element.RelativeTo);
            var relativeFact = Column.GetCell(relativeConcept);

            foreach (var concept in Element.Concepts)
            {
                var fact = Column.GetCell(concept);
                var value = fact.NumericValue.Value;

                string parentId = null;


                var relativeRatio = value / relativeFact.NumericValue.Value;
                if (Math.Abs(relativeRatio) > 0)
                {
                    data.Add(
                        new
                        {
                            id = concept.Name,
                            parentId,
                            //label = concept.Name + " " + concept.ShortLabel,
                            label = concept.ShortLabel,
                            value = concept.ValueFormatted(fact),
                            relativeTo = relativeRatio
                        }
                    );


                    var childData = flatten(concept);
                    data.AddRange(childData);
                    //data.AddRange(childData.Edges);
                }
            }

            return data;
        }

        private List<object> flatten(Concept concept)
        {

            var relativeConcept = Element.AllConcepts.SingleOrDefault(x => x.Name == Element.RelativeTo);
            var relativeFact = Column.GetCell(relativeConcept);

            var data = new List<object>();

            if (concept.Children == null)
            {
                return data;
            }

            foreach (var child in concept.Children)
            {
                var fact = Column.GetCell(child);
                var value = fact == null ? 0 : fact.NumericValue.Value;

                var relativeRatio = value / relativeFact.NumericValue.Value;

                //if (fact != null && fact.NumericValue.HasValue && Math.Abs(fact.NumericValue.Value) >= Limit)
                if (Math.Abs(relativeRatio) > 0)
                {
                    var conceptNode = new
                    {
                        id = child.Name,
                        parentId = concept.Name,
                        //label = child.Name + " " + child.ShortLabel,
                        label = child.ShortLabel,
                        value = child.ValueFormatted(fact),
                        relativeTo = relativeRatio//value / relativeFact.NumericValue.Value

                    };

                    data.Add(conceptNode);

                    var childData = flatten(child);
                    data.AddRange(childData);
                }
            }

            return data;
        }

        //public Data Flatten()
        //{
        //    var data = new Data();

        //    foreach (var concept in Concepts)
        //    {
        //        var fact = Column.GetCell(concept);


        //        data.Nodes.Add(
        //            new ConceptNode
        //            {
        //                NodeId = concept.Name,
        //                Label = concept.ShortLabel,
        //                Value = concept.ValueFormatted(fact)
        //            }
        //        );


        //        var childData = flatten(concept);
        //        data.Nodes.AddRange(childData.Nodes);
        //        data.Edges.AddRange(childData.Edges);
        //    }

        //    return data;
        //}

        //private Data flatten(Concept concept)
        //{

        //    var data = new Data();

        //    if (concept.Children == null)
        //    {
        //        return data;
        //    }

        //    foreach (var child in concept.Children)
        //    {
        //        var fact = Column.GetCell(concept);

        //        var conceptNode = new ConceptNode
        //        {
        //            NodeId = child.Name,
        //            Label = child.ShortLabel,
        //            Value = concept.ValueFormatted(fact)
        //        };

        //        data.Nodes.Add(
        //            conceptNode
        //        );

        //        var edge = new Edge
        //        {
        //            SourceId = concept.Name,
        //            TargetId = child.Name
        //        };

        //        data.Edges.Add(edge);

        //        var childData = flatten(child);
        //        data.Nodes.AddRange(childData.Nodes);
        //        data.Edges.AddRange(childData.Edges);
        //    }

        //    return data;
        //}

    }
}