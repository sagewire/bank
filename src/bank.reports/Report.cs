using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.reports.charts;
using bank.extensions;
using XmlTransform;
using bank.poco;
using bank.utilities;
using bank.data.repositories;
using bank.reports.formulas;
using System.Diagnostics;

namespace bank.reports
{
    public class Report
    {
        public string Partial { get; set; }
        public string Template { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
        public List<Concept> Concepts { get; set; } = new List<Concept>();
        public List<Column> Columns { get; set; } = new List<Column>();
        public IList<ChartConfig> Charts { get; internal set; } = new List<ChartConfig>();

        private Guid _reportId = Guid.NewGuid();
        public Guid ReportId
        {
            get
            {
                return _reportId;
            }
        }

        public Report() { }

        public Report(string template, IList<Column> columns, IDictionary<string, string> placeholders = null, string section = null)
        {
            Placeholders = placeholders;
            Columns.AddRange(columns);
            Template = template;
            Parse(section);
        }

        public DateTime Period { get; set; }
        public DateTime MaxPeriod { get; set; }

        public bool IsCurrentPeriod { get; set; }

        public string Title
        {
            get
            {
                if (CurrentSection != null && !string.IsNullOrWhiteSpace(CurrentSection.Label))
                {
                    return CurrentSection.Label;
                }
                return null;
            }
        }

        private Section _currentSection = null;
        public Section CurrentSection
        {
            get
            {
                if (_currentSection == null)
                {
                    _currentSection = Sections.First();
                }
                return _currentSection;
            }
            set
            {
                _currentSection = value;
            }
        }

        public void Parse(string sectionName = null)
        {
            var xmlTemplate = GetTemplate();
            ParseSections((XElement)xmlTemplate.FirstNode, sectionName);
        }

        private void ParseSections(XElement element, string sectionName = null)
        {
            sectionName = sectionName?.ToLower();

            var rootName = element.Attribute("root");

            Partial = element.SafeAttributeValue("partial") ?? "_Report";

            if (rootName != null)
            {
                element = element.Elements().SingleOrDefault(x => x.Attribute("name").Value == rootName.Value);
            }

            IEnumerable<XElement> children;

            
            children = element.Elements();
            

            foreach (var childElement in children)
            {
                ParseSection(childElement, sectionName);
            }
        }

        public static void PopulateReport(Report report)
        {
            PopulateReports(new Report[] { report });
        }

        private static void PopulateDefinitions(IList<Report> reports, IList<string> conceptKeys)
        {
            var conceptRepo = new ConceptDefinitionRepository();
            //var filtered = conceptKeys.Where(x => !Global.Concepts.ContainsKey(x));

            var definitions = conceptRepo.Filter(conceptKeys);

            foreach (var report in reports.Where(x => x != null))
            {
                report.SetDefinitions(definitions);
            }

        }

        private void SetDefinitions(List<ConceptDefinition> definitions)
        {
            foreach (var concept in Concepts)
            {
                var definition = definitions.SingleOrDefault(x => x.Mdrm == concept.Value);

                if (definition != null)
                {
                    concept.SetValues(definition);
                }

                if (Global.Concepts.ContainsKey(concept.Value))
                {
                    var global = Global.Concepts[concept.Value];
                    concept.SetValues(global);
                }
            }
        }

        public static void PopulateReports(IList<Report> reports)
        {
            var conceptKeys = new List<string>();
            var orgs = new List<int>();

            foreach (var report in reports.Where(x => x != null))
            {
                conceptKeys.AddRange(report.ConceptKeys);
                orgs.AddRange(report.AllOrganizations());
            }

            conceptKeys = conceptKeys.Distinct().ToList();

            var tasks = new List<Task>();

            foreach (var report in reports.Where(x => x != null))
            {
                tasks.Add(Task.Run(() =>
                {
                    PopulateFacts(report);
                }));
            }

            tasks.Add(Task.Run(() =>
            {
                PopulateDefinitions(reports, conceptKeys);
            }));

            Task.WaitAll(tasks.ToArray());

        }


        private static void PopulateFacts(Report report)
        {
            var factRepo = new FactRepository();

            var facts = factRepo.GetFacts(report.ConceptKeys, report.AllOrganizations(), report.Period);//, lookback: DateTime.Now.AddQuarters(-12));

            report.MaxPeriod = facts.Max(x => x.Period.Value);

            report.SetFacts(facts);
            
        }

        public void SetFacts(IList<Fact> facts)
        {
            foreach (var column in Columns)
            {
                column.SetFacts(facts);
            }
        }

        public static Fact GetCell(LineItem lineItem, Column column)
        {
            if (!lineItem.Concepts.Any()) return null;

            foreach (var concept in lineItem.Concepts)
            {
                var facts = column.GetFacts(concept.ConceptKeys);
                var fact = concept.PrepareFact(facts);

                return fact;
            }

            return null;
        }

        private void ParseSection(XElement element, string sectionFilter)
        {
            var section = new Section();

            SetCommonAttributes(section, element);

            Sections.Add(section);

            if (sectionFilter == null || sectionFilter == section.Id.ToLower())
            {
                ParseElements(element, section);

                if (sectionFilter == section.Id?.ToLower())
                {
                    CurrentSection = section;
                }
            }

        }
        private void ParseSubSection(XElement element, Section section)
        {
            var subSection = new SubSection();
            SetCommonAttributes(subSection, element);

            if (section.SubSections == null) section.SubSections = new List<SubSection>();

            section.SubSections.Add(subSection);
            section.LineItems.Add(subSection);

            ParseElements(element, subSection);

        }

        private void ParseElements(XElement element, LineItem parent)
        {
            foreach (var child in element.Elements())
            {
                ParseElement(child, parent);
            }
        }

        private void ParseElement(XElement element, LineItem parent)
        {
            switch (element.Name.LocalName)
            {
                case "line":
                    ParseLine(element, parent);
                    break;
                case "charts":
                    ParseCharts(element, parent);
                    break;
                case "chart":
                    ParseChart(element, parent);
                    break;
            }

            //foreach (var child in element.Elements("line"))
            //{
            //    ParseElement(child, parent);
            //}

        }

        private void ParseLine(XElement element, LineItem lineItem)
        {
            var type = element.SafeAttributeValue("type");

            switch (type)
            {
                case "subsection":
                    ParseSubSection(element, (Section)lineItem);
                    return;
            }

            ParseTable(element, lineItem);
        }

        private void ParseTable(XElement element, LineItem lineItem)
        {
            var isAbstract = element.SafeBoolAttributeValue("abstract");

            if (isAbstract.HasValue && !isAbstract.Value)
            {

                var name = element.SafeAttributeValue("name");
                var concept = new Concept(name);
                lineItem.Concepts.Add(concept);
                this.Concepts.Add(concept);
            }
            else
            {

                var childLineItem = new TableItem();
                SetCommonAttributes(childLineItem, element);
                lineItem.LineItems.Add(childLineItem);

                ParseElements(element, childLineItem);

            }
        }

        private void ParseCharts(XElement element, LineItem lineItem)
        {
            var charts = new Charts();

            foreach (var chart in element.Elements("chart"))
            {
                ParseChart(chart, charts);
            }

            if (lineItem.LineItems == null) lineItem.LineItems = new List<LineItem>();

            lineItem.LineItems.Add(charts);

        }

        private void ParseChart(XElement element, LineItem lineItem)
        {
            var chart = new ChartItem();

            if (lineItem.LineItems == null) lineItem.LineItems = new List<LineItem>();

            chart.ChartConfig = ChartConfig.Build(element, Placeholders);

            chart.Concepts.AddRange(chart.ChartConfig.Concepts);

            lineItem.LineItems.Add(chart);

            if (Concepts == null) Concepts = new List<Concept>();

            Concepts.AddRange(chart.Concepts);

            Charts.Add(chart.ChartConfig);

        }

        public List<int> AllOrganizations()
        {
            var orgs = new List<int>();

            foreach (var column in Columns)
            {
                var companyColumn = column as CompanyColumn;
                if (companyColumn != null)
                {
                    orgs.Add(companyColumn.OrganizationId);
                }
            }

            return orgs;
        }

        private List<string> _conceptKeys;
        public IList<string> ConceptKeys
        {
            get
            {
                if (_conceptKeys == null)
                {
                    var conceptKeys = new List<string>();

                    foreach (var concept in Concepts)
                    {
                        conceptKeys.AddRange(concept.ConceptKeys);
                    }

                    _conceptKeys = conceptKeys;
                }
                return _conceptKeys;
            }
        }

        public IDictionary<string, string> Placeholders { get; set; }

        private void SetCommonAttributes(ICommonAttributes obj, XElement element)
        {
            obj.Id = element.SafeAttributeValue("name")?.ToLower();
            obj.Label = element.SafeAttributeValue("label");
        }


        private XDocument GetTemplate()
        {
            var configLocation = Path.Combine(Settings.ReportTemplatePath, Template + ".xml");
            var patchLocation = configLocation.Replace(".xml", ".patch.xml");

            var xmlText = File.ReadAllText(configLocation);

            if (File.Exists(patchLocation))
            {
                var patchText = File.ReadAllText(patchLocation);

                var transformer = new XmlTransformer();
                var source = xmlText;
                var transform = patchText;

                var result = string.Empty;

                xmlText = transformer.ApplyTransform(source, transform);

            }

            var xml = XDocument.Parse(xmlText);

            return xml;
        }
    }
}
