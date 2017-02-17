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
        public static List<Annotation> ToAnnotations(this List<OrganizationFfiecTransformation> list)
        {
            var annotations = new List<Annotation>();

            foreach(var item in list)
            {
                var annotation = new Annotation();
                annotation.Date = item.D_DT_TRANS.Value;

                if (item.PredecessorOrganization != null)
                {
                    annotation.Text = string.Format("{0} Merger", item.PredecessorOrganization.Name);
                }
                else
                {
                    annotation.Text = "Unknown Merger";
                }

                annotations.Add(annotation);
            }
            return annotations;
        }
    }
}
