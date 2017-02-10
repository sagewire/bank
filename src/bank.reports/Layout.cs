using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.data.repositories;
using bank.extensions;
using bank.poco;
using bank.reports.charts;
using XmlTransform;

namespace bank.reports
{
    public class Layout
    {
        public string Template { get; set; }
        public string Name { get; set; }
        public string SectionFilter { get; set; }

        public IList<TemplateRow> Rows { get; set; }
        public IDictionary<string, string> Placeholders { get; private set; }

        public IList<Column> DataColumns { get; set; }
        public List<Concept> Concepts { get; set; } = new List<Concept>();

        public List<FactLookup> FactLookups { get; set; } = new List<FactLookup>();

        public List<TemplateElement> Elements { get; set; } = new List<TemplateElement>();

        public Layout(string template = null)
        {
            Template = template;
        }


        public void Populate(DateTime periodEnd)
        {
            var tasks = new List<Task>();

            foreach (var element in Elements)
            {
                element.SetDataColumns(DataColumns);
                FactLookups = FactLookup.Merge(element.FactLookups, this.FactLookups);
                Concepts.AddRange(element.Concepts);
            }



            var factTask = Task.Run(() =>
            {
                var facts = GetFacts(FactLookups, periodEnd);

                foreach (var column in DataColumns)
                {
                    column.SetFacts(facts);
                }

            });

            tasks.Add(factTask);

            tasks.Add(Task.Run(() =>
            {
                SetDefinitions();
            }));

            Task.WaitAll(tasks.ToArray());
            
            foreach(var task in tasks)
            {
                if (task.IsFaulted)
                {
                    throw task.Exception;
                }
            }
                        
        }
        
        private void SetDefinitions()
        {
            var conceptRepo = new ConceptDefinitionRepository();
            //var filtered = conceptKeys.Where(x => !Global.Concepts.ContainsKey(x));

            var conceptKeys = Concept.GetConceptKeys(Concepts);

            var definitions = conceptRepo.Filter(conceptKeys);

            foreach (var concept in Concepts)
            {
                var definition = definitions.SingleOrDefault(x => x.Mdrm == concept.Value);

                if (definition != null)
                {
                    concept.SetValues(definition);
                }

                if (concept.Name != null && Global.Concepts.ContainsKey(concept.Name))
                {
                    var global = Global.Concepts[concept.Name];
                    concept.SetValues(global);
                }
            }
        }


        private IList<Fact> GetFacts(IList<FactLookup> lookups, DateTime period)
        {
            var tasks = new List<Task<IList<Fact>>>();

            foreach (var lookup in lookups)
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

        public void Load(string template = null)
        {
            if (!string.IsNullOrWhiteSpace(template))
            {
                Template = template;
            }

            var xml = LoadTemplate();
            Parse((XElement)xml.FirstNode, SectionFilter);
        }


        private void Parse(XElement layout, string sectionName = null)
        {
            sectionName = sectionName?.ToLower();

            var rows = layout.Elements("row");

            this.Rows = ParseRows(rows);
        }

        private IList<TemplateRow> ParseRows(IEnumerable<XElement> rows)
        {
            var templateRows = new List<TemplateRow>();

            foreach (var row in rows)
            {
                var templateRow = ParseRow(row);
                templateRows.Add(templateRow);
            }

            return templateRows;
        }

        private TemplateRow ParseRow(XElement row)
        {
            var templateRow = new TemplateRow();

            var cols = row.Elements("col");

            templateRow.Columns = ParseCols(cols);

            return templateRow;
        }

        private IList<TemplateColumn> ParseCols(IEnumerable<XElement> cols)
        {
            var templateColumns = new List<TemplateColumn>();

            foreach (var col in cols)
            {
                var templateCol = ParseCol(col);
                templateColumns.Add(templateCol);
            }

            return templateColumns;
        }

        private TemplateColumn ParseCol(XElement col)
        {
            var templateColumn = new TemplateColumn();
            templateColumn.CssClasses = col.SafeAttributeValue("css");
            templateColumn.GridOverride = col.SafeAttributeValue("grid");

            //var isChild = col.SafeBoolAttributeValue("child");

            //if (isChild.HasValue)
            //{
            //    templateColumn.IsChild = isChild.Value;
            //}

            var elements = col.Elements();

            templateColumn.Elements = ParseElements(elements);

            var rows = col.Elements("row");

            templateColumn.Rows = ParseRows(rows);

            return templateColumn;
        }

        private IList<TemplateElement> ParseElements(IEnumerable<XElement> elements)
        {
            var templateElements = new List<TemplateElement>();

            foreach (var element in elements)
            {
                var templateElement = ParseElement(element);

                if (templateElement != null)
                {
                    templateElements.Add(templateElement);
                    Elements.Add(templateElement);
                }
            }

            return templateElements;
        }

        private TemplateElement ParseElement(XElement element)
        {
            var name = element.Name.ToString().ToLower();
            var partial = element.SafeAttributeValue("partial");
            var title = element.SafeAttributeValue("title");
            var lookback = element.SafeIntAttributeValue("lookback");

            TemplateElement item;

            switch (name)
            {
                case "row":
                    return null;
                case "html":
                    item = ParseHtmlElement(element);
                    break;
                case "chart":
                    item = ParseChart(element);
                    break;
                case "table":
                    item = ParseTable(element);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The element [{0}] is not supported", name));
            }

            item.Title = title;
            item.Partial = partial;
            item.Lookback = lookback;
            

            return item;

        }

        private ChartElement ParseChart(XElement element)
        {
            var chart = new ChartElement();

            chart.ChartConfig = ChartConfig.Build(element, Placeholders);

            chart.Concepts.AddRange(chart.ChartConfig.Concepts);
            
            //this.Elements.Add(chart);

            return chart;

        }

        private TableElement ParseTable(XElement element, int level = 0)
        {
            var table = new TableElement();
            table.Level = level;
                        table.Orientation = Enum.Parse(typeof(TableOrientation), element.SafeAttributeValue("orientation") ?? "vertical", true);
            TableElement current = table;
            

            foreach(var item in element.Elements())
            {
                ITableRow tableRow = null;

                switch(item.Name.ToString().ToLower())
                {
                    case "concept":
                        var conceptRow = new TableRow();
                        conceptRow.TableRowType = TableRowTypes.Concept;
                        conceptRow.Concept = new Concept(item.SafeAttributeValue("name"));
                        conceptRow.Concept.Label = item.SafeAttributeValue("label") ?? conceptRow.Concept.Label;
                        conceptRow.Concept.Unit = item.SafeCharAttributeValue("unit");
                        conceptRow.Concept.Negative = item.SafeBoolAttributeValue("negative");

                        current.Concepts.Add(conceptRow.Concept);
                        tableRow = conceptRow;
                        break;
                    case "group":
                        var groupRow = new TableRowGroup();
                        groupRow.Label = item.SafeAttributeValue("label");
                        groupRow.Sum = item.SafeBoolAttributeValue("sum") ?? false;
                        groupRow.Table = ParseTable(item, level + 1);
                        Elements.Add(groupRow.Table);
                        tableRow = groupRow;
                        //current = groupRow;
                        break;
                    case "header":
                        var headerRow = new TableRow();
                        headerRow.TableRowType = TableRowTypes.Header;
                        headerRow.Label = item.Value;

                        tableRow = headerRow;
                        break;
                }

                if (tableRow != null)
                {
                    current.Rows.Add(tableRow);
                }
            }

            return table;
            //var sb = new StringBuilder();

            //var html = new HtmlElement();

            //foreach (var node in element.Nodes())
            //{
            //    sb.AppendLine(node.ToString());
            //}

            //html.Content = sb.ToString();

            //return html;
        }

        private HtmlElement ParseHtmlElement(XElement element)
        {
            var sb = new StringBuilder();

            var html = new HtmlElement();

            foreach (var node in element.Nodes())
            {
                sb.AppendLine(node.ToString());
            }

            html.Content = sb.ToString();

            return html;
        }

        private XDocument LoadTemplate()
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
