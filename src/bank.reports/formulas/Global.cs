using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;

namespace bank.reports.formulas
{
    public static class Global
    {
        private static IDictionary<string, Concept> _concepts = new Dictionary<string, Concept>();

        static Global()
        {
            var configLocation = Path.Combine(Settings.ReportTemplatePath, "config.xml");

            var xmlText = File.ReadAllText(configLocation);

            var xml = XDocument.Parse(xmlText);

            var concepts = xml.Element("configuration").Element("concepts").Elements("concept");

            foreach(var conceptElement in concepts)
            {
                var name = conceptElement.SafeAttributeValue("name");
                var shortLabel = conceptElement.SafeAttributeValue("shortLabel");
                var formula = conceptElement.SafeAttributeValue("formula") ?? conceptElement.SafeAttributeValue("name");
                var label = conceptElement.SafeAttributeValue("label");
                var narrative = conceptElement.SafeAttributeValue("narrative");

                var concept = new Concept(formula);
                concept.Name = name;
                concept.ShortLabel = shortLabel;
                concept.Label = label;
                concept.Narrative = narrative;
                concept.Value = formula;

                _concepts.Add(name, concept);

            }
        }

        public static IDictionary<string, Concept> Concepts
        {
            get
            {
                return _concepts;
            }
        }
    }
}
