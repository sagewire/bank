using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bank.poco;
using bank.reports.charts;
using bank.extensions;

namespace bank.reports.extensions
{
    public static class AnnotationExtensions
    {
        public static Annotations ToAnnotations(this List<OrganizationFfiecTransformation> list, string name, string id)
        {
            var annotations = new Annotations();
            annotations.Items = new List<Annotation>();
            annotations.Visible = list.Count < 100;

            foreach (var item in list)
            {
                var annotation = new Annotation();
                annotation.Date = item.D_DT_TRANS.Value;

                if (item.PredecessorOrganization != null)
                {
                    annotation.Text = item.PredecessorOrganization.Name;
                }
                else
                {
                    annotation.Text = "Unknown Merger";
                }

                annotations.Name = name;
                annotations.Items.Add(annotation);
            }
            return annotations;
        }

        public static Annotations ToAnnotations(this List<OrganizationFfiecRelationship> list, string name, string id)
        {
            var annotations = new Annotations();
            annotations.Items = new List<Annotation>();
            annotations.Visible = list.Count < 100;

            foreach (var item in list)
            {
                var annotation = new Annotation();
                annotation.Date = item.DateRelationshipStart.Value;

                
                annotation.Text = item.OffspringOrganization?.Name ?? item.ParentOrganization?.Name ?? "Unknown";
            
                annotations.Name = name;
                annotations.Items.Add(annotation);
            }
            return annotations;
        }
    }
}
