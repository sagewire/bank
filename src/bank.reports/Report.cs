﻿using System;
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
        public List<Column> Columns { get; set; } = new List<Column>();
        public IList<ChartConfig> Charts { get; internal set; } = new List<ChartConfig>();
        public int? Lookback { get; set; }

        public List<Concept> ReportConcepts { get; set; } = new List<Concept>();

        private List<Concept> _combined;
        public List<Concept> Concepts {
            get
            {
                if (_combined == null)
                {
                    var combined = new List<Concept>();
                    combined.AddRange(ReportConcepts);

                    foreach (var chart in Charts)
                    {
                        combined.AddRange(chart.Concepts);
                    }

                    _combined = combined;
                }
                return _combined;
            }
        }


        private Guid _reportId = Guid.NewGuid();
        public Guid ReportId
        {
            get
            {
                return _reportId;
            }
        }

        public IList<Column> VisibleColumns
        {
            get
            {
                return Columns.Where(x => x.ShowColumn == true).ToList();
            }
        }

        private IList<CompanyColumn> _companyColumns;
        public IList<CompanyColumn> CompanyColumns
        {
            get
            {
                return _companyColumns = Columns.Where(x => x.ColumnType == ColumnTypes.Company)
                                .Select(x => (CompanyColumn)x)
                                .ToList();
            }
        }


        private IList<PeerGroupColumn> _peerGroupColumns;
        public IList<PeerGroupColumn> PeerGroupColumns
        {
            get
            {
                return _peerGroupColumns = Columns.Where(x => x.ColumnType == ColumnTypes.PeerGroup)
                                .Select(x => (PeerGroupColumn)x)
                                .ToList();
            }
        }

        public Report() { }

        public Report(string template, IList<Column> columns = null, IDictionary<string, string> placeholders = null, string section = null)
        {
            Placeholders = placeholders;

            if (columns != null) Columns.AddRange(columns);

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
            Lookback = element.SafeIntAttributeValue("lookback");

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

        public static void PopulateReport(Report report, DateTime periodStart, DateTime periodEnd)
        {
            PopulateReports(new Report[] { report }, report.Columns, periodStart, periodEnd);
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

        public static void PopulateReports(IList<Report> reports, IList<Column> columns, DateTime periodStart, DateTime periodEnd)
        {
            var conceptKeys = new List<string>();
            var orgs = new List<int>();
            var peerGroups = new List<string>();

            var lookups = new List<FactLookup>();

            foreach (var report in reports.Where(x => x != null))
            {
                lookups = FactLookup.Merge(report.FactLookups, lookups);

                conceptKeys.AddRange(report.ConceptKeys);
                orgs.AddRange(report.AllOrganizations());
                peerGroups.AddRange(report.AllPeerGroups());
            }

            conceptKeys = conceptKeys.Distinct().ToList();

            //var factRepo = new FactRepository();

            //var facts = factRepo.GetFacts(conceptKeys, orgs);//, lookback: DateTime.Now.AddQuarters(-12));


            var tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
                //var facts = GetFacts(lookups, columns, conceptKeys, orgs, peerGroups, periodStart, periodEnd);
                var facts = GetFacts(lookups, periodEnd);

                foreach (var report in reports.Where(x => x != null))
                {
                    report.MaxPeriod = facts.Max(x => x.Period.Value);
                    report.SetFacts(facts);
                }
            }));


            tasks.Add(Task.Run(() =>
            {
                PopulateDefinitions(reports, conceptKeys);
            }));

            Task.WaitAll(tasks.ToArray());

        }

        //private static IList<Fact> GetFacts(IList<FactLookup> lookups, IList<Column> columns, IList<string> conceptKeys, IList<int> orgs, IList<string> peerGroups, DateTime period)
        private static IList<Fact> GetFacts(IList<FactLookup> lookups, DateTime period)
        {
            var tasks = new List<Task<IList<Fact>>>();

            foreach(var lookup in lookups)
            {
                if (lookup.Columns.Any(x => x.ColumnType == ColumnTypes.Company))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var factRepo = new FactRepository();
                        //return factRepo.GetFacts(conceptKeys, orgs, periodStart, periodEnd);//, lookback: DateTime.Now.AddQuarters(-12));

                        return factRepo.GetFacts(lookup.ConceptKeys, lookup.OrganizationIds, period, lookup.Lookback);//, lookback: DateTime.Now.AddQuarters(-12));
                    }));
                }

                if (lookup.Columns.Any(x => x.ColumnType == ColumnTypes.PeerGroup))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var factRepo = new FactRepository();
                        return factRepo.GetPeerGroupFacts(lookup.ConceptKeys, lookup.PeerGroups, period, lookup.Lookback);//, lookback: DateTime.Now.AddQuarters(-12));
                    }));
                }

                if (lookup.Columns.Any(x => x.ColumnType == ColumnTypes.PeerGroupCustom))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var column = lookup.Columns.First(x => x.ColumnType == ColumnTypes.PeerGroupCustom) as PeerGroupCustomColumn;

                        var factRepo = new FactRepository();
                        return factRepo.GetPeerGroupCustomFacts(lookup.ConceptKeys, column.PeerGroupCustom, period, lookup.Lookback);//, lookback: DateTime.Now.AddQuarters(-12));
                    }));
                }
            }



            Task.WaitAll(tasks.ToArray());

            var facts = new List<Fact>();

            foreach (var task in tasks)
            {
                facts.AddRange(task.Result);
            }

            return facts;
        }

        //private static void PopulateFacts(Report report)
        //{
        //    var factRepo = new FactRepository();

        //    var facts = factRepo.GetFacts(report.ConceptKeys, report.AllOrganizations(), report.Period);//, lookback: DateTime.Now.AddQuarters(-12));

        //    report.MaxPeriod = facts.Max(x => x.Period.Value);

        //    var peerGroupFacts = factRepo.GetPeerGroupFacts(report.ConceptKeys, report.AllPeerGroups(), report.Period);

        //    report.SetFacts(facts);
        //    report.SetFacts(peerGroupFacts);

        //}

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
                this.ReportConcepts.Add(concept);
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

            //if (Concepts == null) Concepts = new List<Concept>();

            //Concepts.AddRange(chart.Concepts);

            Charts.Add(chart.ChartConfig);

        }

        public List<string> AllPeerGroups()
        {
            var peerGroups = new List<string>();

            foreach (var column in Columns)
            {
                var peerGroupColumn = column as PeerGroupColumn;
                if (peerGroupColumn != null)
                {
                    peerGroups.Add(peerGroupColumn.PeerGroup);
                }
            }

            return peerGroups;
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

        private List<FactLookup> _factLookups;
        public IList<FactLookup> FactLookups
        {
            get
            {
                if (_factLookups == null)
                {
                    var lookups = new List<FactLookup>();

                    var reportLookup = new FactLookup
                    {
                        Lookback = this.Lookback,
                        Columns = this.Columns,
                        ConceptKeys = Concept.GetConceptKeys(this.ReportConcepts)
                    };

                    if (reportLookup.ConceptKeys.Any())
                    {
                        lookups.Add(reportLookup);
                    }
                    
                    foreach(var chart in this.Charts)
                    {
                        chart.Columns = this.Columns;
                        
                        lookups = FactLookup.Merge(chart.FactLookups, lookups);
                    }

                    _factLookups = lookups;
                }
                return _factLookups;
            }
        }

        private List<string> _conceptKeys;
        public IList<string> ConceptKeys
        {
            get
            {
                if (_conceptKeys == null)
                {
                    _conceptKeys = Concept.GetConceptKeys(Concepts);
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
