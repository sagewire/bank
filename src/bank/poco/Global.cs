using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.extensions;

namespace bank.poco
{
    public static class Global
    {
        private static IDictionary<string, Concept> _concepts = new Dictionary<string, Concept>();
        private static IDictionary<string, PeerGroupStandard> _peerGroups = new Dictionary<string, PeerGroupStandard>();

        static Global()
        {
            InitConcepts();
            InitPeerGroups();
        }


        private static void InitConcepts()
        {
            var configLocation = Path.Combine(Settings.ReportTemplatePath, "config.xml");

            var xmlText = File.ReadAllText(configLocation);

            var xml = XDocument.Parse(xmlText);

            var concepts = xml.Element("configuration").Element("concepts").Elements("concept");

            foreach (var conceptElement in concepts)
            {
                var name = conceptElement.SafeAttributeValue("name").ToUpper();
                var shortLabel = conceptElement.SafeAttributeValue("shortLabel");
                var formula = conceptElement.SafeAttributeValue("formula") ?? conceptElement.SafeAttributeValue("name");
                var label = conceptElement.SafeAttributeValue("label");
                var narrative = conceptElement.SafeAttributeValue("narrative");
                var unit = conceptElement.SafeAttributeValue("unit");

                var concept = new Concept(formula);
                concept.Name = name;
                concept.ShortLabel = shortLabel;
                concept.Label = label;
                concept.Narrative = narrative;
                concept.Value = formula;
                concept.Unit = !string.IsNullOrWhiteSpace(unit) ? (char?)unit.ToCharArray()[0] : null;

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


        public static IDictionary<string, PeerGroupStandard> PeerGroups
        {
            get
            {
                return _peerGroups;
            }
        }

        private static void InitPeerGroups()
        {
            var configLocation = Path.Combine(Settings.ReportTemplatePath, "peer-groups.xml");

            var xmlText = File.ReadAllText(configLocation);

            var xml = XDocument.Parse(xmlText);

            var peergroups = xml.Element("peergroups").Elements("peergroup");

            foreach (var peerGroupElement in peergroups)
            {
                var code = peerGroupElement.Element("code").Value;
                var name = peerGroupElement.Element("name").Value;
                var labelNode = peerGroupElement.Element("label");

                var label = labelNode == null ? name : labelNode.Value;

                var pg = new PeerGroupStandard
                {
                    Code = code,
                    Name = name,
                    Label = label ?? name
                };

                _peerGroups.Add(code, pg);

            }
        }
    }
}
