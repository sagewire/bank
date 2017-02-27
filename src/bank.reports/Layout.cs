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
using bank.reports.extensions;
using XmlTransform;

namespace bank.reports
{
    public delegate void DataSourceNeededEventHandler(TemplateElement element);

    public class Layout
    {
        public event DataSourceNeededEventHandler DataSourceNeeded;
        public string Template { get; set; }
        public string Name { get; set; }
        public string SectionFilter { get; set; }

        public IList<TemplateRow> Rows { get; set; }
        public IDictionary<string, string> Placeholders { get; private set; }

        public List<Column> DataColumns { get; set; }

        public List<Concept> Concepts { get; set; } = new List<Concept>();

        public List<FactLookup> FactLookups { get; set; } = new List<FactLookup>();

        public List<TemplateElement> Elements { get; set; } = new List<TemplateElement>();
        public Header Header { get; private set; }
        public Dictionary<string, object> Parameters { get; internal set; }
        public List<string> Scripts { get; private set; }
        public List<string> Styles { get; private set; }

        public Layout(string template = null)
        {
            Template = template;
        }

        protected virtual void OnDataSourceNeeded(TemplateElement element)
        {
            if (DataSourceNeeded != null)
                DataSourceNeeded(element);
        }



        public void Populate(DateTime periodEnd)
        {
            var tasks = new List<Task>();
            var dataSourceElements = new List<TemplateElement>();

            foreach (var element in Elements)
            {
                element.SetDataColumns(DataColumns);
                FactLookups = FactLookup.Merge(element.FactLookups, this.FactLookups);
                Concepts.AddRange(element.AllConcepts);

                if(!string.IsNullOrWhiteSpace(element.DataSource))
                {
                    dataSourceElements.Add(element);
                }
            }

            var factTask = Task.Run(() =>
            {
                var facts = GetFacts(FactLookups, periodEnd);

                var childColumns = new List<Column>();

                foreach (var column in DataColumns)
                {
                    column.SetFacts(facts, Concepts);

                    if (column.ChildColumns != null && column.ChildColumns.Any())
                    {
                        childColumns.AddRange(column.ChildColumns);
                    }
                }

                DataColumns.AddRange(childColumns);

            });

            var dataSourcesTask = Task.Run(() =>
            {
                foreach(var element in dataSourceElements)
                {
                    OnDataSourceNeeded(element);
                }
            });

            tasks.Add(factTask);

            tasks.Add(Task.Run(() =>
            {
                SetDefinitions();
            }));

            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    throw task.Exception;
                }
            }

            //foreach (var element in Elements)
            //{
            //    element.SetDataColumns(DataColumns);
            //}

        }

        //private List<Column> _visibleColumns;
        //public List<Column> VisibleColumns
        //{
        //    get
        //    {
        //        return _visibleColumns = DataColumns.Where(x => x.ShowColumn).ToList();
        //    }
        //}


        private void SetDefinitions()
        {
            var conceptRepo = new ConceptDefinitionRepository();
            //var filtered = conceptKeys.Where(x => !Global.Concepts.ContainsKey(x));

            var conceptKeys = Concept.GetConceptKeys(Concepts);

            var definitions = conceptRepo.Filter(conceptKeys);

            foreach (var concept in Concepts)
            {
                var definition = definitions.SingleOrDefault(x => x.ItemNumber == concept.ItemNumber);

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

            var conceptTitles = Elements.Where(x => x.Title != null && x.Title.Contains(".concept."));
            foreach(var title in conceptTitles)
            {
                title.Title = title.Title.ConceptReplace(Concepts);
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


                if (lookup.Columns.Any(x => x.ColumnType == ColumnTypes.CompanyRank))
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var factRepo = new FactRepository();
                        return factRepo.GetFacts(lookup.ConceptKeys, period, 0);//, lookback: DateTime.Now.AddQuarters(-12));
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
            XElement node = xml.Root as XElement;
            //if (xml.FirstNode as XElement != null)
            //{
            //    node = (XElement)xml.FirstNode;
            //}
            //else
            //{
            //    node = (XElement)xml.NextNode;
            //}

            Parse((XElement)node, SectionFilter);
        }


        private void Parse(XElement layout, string sectionName = null)
        {
            sectionName = sectionName?.ToLower();

            ParseIncludes(layout);

            var header = layout.Elements("header").FirstOrDefault();

            this.Header = ParseHeader(header);

            this.Scripts = ParseScripts(layout);
            this.Styles = ParseStyles(layout);

            var rows = layout.Elements("row");

            this.Rows = ParseRows(rows);
        }

        private void ParseIncludes(XElement layout)
        {
            var includes = layout.Descendants("include").ToList();

            foreach(var include in includes)
            {
                var file = include.SafeAttributeValue("file");
                var path = Path.Combine(Settings.ReportTemplatePath, file);

                var fragment = XDocument.Load(path);
                include.ReplaceWith(fragment.Root);

            }
        }

        private List<string> ParseScripts(XElement layout)
        {
            var scripts = new List<string>();
            var scriptElems = layout.Elements("script");

            foreach (var item in scriptElems)
            {
                var src = item.SafeAttributeValue("src");
                if (!string.IsNullOrWhiteSpace(src))
                {
                    scripts.Add(src);
                }
            }

            return scripts;
        }

        private List<string> ParseStyles(XElement layout)
        {
            var styles = new List<string>();
            var styleElems = layout.Elements("style");

            foreach (var item in styleElems)
            {
                var src = item.SafeAttributeValue("src");
                if (!string.IsNullOrWhiteSpace(src))
                {
                    styles.Add(src);
                }
            }

            return styles;
        }

        private Header ParseHeader(XElement headerElement)
        {
            if (headerElement == null) return null;

            var header = new Header();
            header.Partial = headerElement.SafeAttributeValue("partial");

            return header;
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
            templateColumn.CssClasses = col.SafeAttributeValue("css-classes");
            templateColumn.GridOverride = col.SafeAttributeValue("grid");
            templateColumn.UseContainer = col.SafeBoolAttributeValue("container") ?? true;
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
            var title = element.SafeAttributeValue("title").ParameterReplace(Parameters);
            var lookback = element.SafeIntAttributeValue("lookback");
            var dataSource = element.SafeAttributeValue("data-source");
            var css = element.SafeAttributeValue("css-classes");

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
                case "timeline":
                    item = ParseTimeline(element);
                    break;
                case "hierarchy":
                    item = ParseHierarchy(element);
                    break;
                case "element":
                    item = ParseElementChildren<TemplateElement>(element);// new TemplateElement();
                    break;
                default:
                    throw new NotSupportedException(string.Format("The element [{0}] is not supported", name));
            }

            item.Title = title;
            item.Partial = partial;
            item.Lookback = lookback;
            item.DataSource = dataSource;
            item.CssClasses = css;
            
            return item;

        }

        private TimelineElement ParseTimeline(XElement element)
        {
            var timeline = new TimelineElement();
            timeline.Limit = (int?)element.SafeLongAttributeValue("limit");
            return timeline;
        }


        private HierarchyElement ParseHierarchy(XElement element)
        {
            var hierarchy = ParseElementChildren<HierarchyElement>(element);
            hierarchy.RelativeTo = element.SafeAttributeValue("relative");
            hierarchy.HiddenConcepts.Add(new Concept(hierarchy.RelativeTo));

            return hierarchy;
        }

        private ChartElement ParseChart(XElement element)
        {
            var chart = new ChartElement();

            chart.ChartConfig = ChartConfig.Build(element, Parameters);

            chart.Concepts.AddRange(chart.ChartConfig.Concepts);

            //this.Elements.Add(chart);

            return chart;

        }

        private T ParseElementChildren<T>(XElement element) where T : TemplateElement, new()
        {
            var elem = new T();

            elem.Concepts = ParseConcepts(element);

            return elem;
        }

        private List<Concept> ParseConcepts(XElement element)
        {
            var result = new List<Concept>();
            var concepts = element.Elements("concept");

            foreach (var conceptElem in concepts)
            {
                var name = conceptElem.SafeAttributeValue("name").ParameterReplace(Parameters).ToUpper();
                var concept = new Concept(name);
                concept.FormatHint = conceptElem.SafeAttributeValue("format");

                concept.Children = ParseConcepts(conceptElem);

                result.Add(concept);

                //var childConcepts = element.Elements("concept");
                //foreach(var childConcept in childConcepts)
                //{
                //    if (concept.Children == null) concept.Children = new List<Concept>();

                //    var children = ParseConcepts(childConcept);
                //    concept.Children.AddRange(children);
                //}

            }

            return result;
        }

        private TableElement ParseTable(XElement element, int level = 0)
        {
            var table = new TableElement();
            table.Level = level;
            table.Orientation = (TableOrientation)Enum.Parse(typeof(TableOrientation), element.SafeAttributeValue("orientation") ?? "horizontal", true);
            table.ShowRank = element.SafeBoolAttributeValue("show-rank") ?? false;
            table.Thousands = element.SafeBoolAttributeValue("thousands") ?? true;

            TableElement current = table;


            foreach (var item in element.Elements())
            {
                ITableRow tableRow = null;

                var n = item.Name.ToString().ToLower();
                switch (n)
                {
                    case "date":
                    case "concept":
                        var conceptRow = new TableRow();
                        conceptRow.TableRowType = n == "concept" ? TableRowTypes.Concept : TableRowTypes.Date;
                        var name = item.SafeAttributeValue("name").ParameterReplace(Parameters).ToUpper();
                        var link = item.SafeBoolAttributeValue("link");

                        if (link.HasValue) {
                            conceptRow.Link = link.Value;
                        }
                        
                        conceptRow.Concept = new Concept(name);

                        conceptRow.Concept.Label = item.SafeAttributeValue("label") ?? conceptRow.Concept.Label;
                        conceptRow.Concept.Unit = item.SafeCharAttributeValue("unit");
                        conceptRow.Concept.Negative = item.SafeBoolAttributeValue("negative");
                        conceptRow.Concept.FormatHint = item.SafeAttributeValue("format");

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
                        headerRow.SubText = item.SafeAttributeValue("subtext");
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
