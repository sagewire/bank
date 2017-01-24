using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using bank.data.repositories;
using bank.enums;
using bank.poco;
using bank.Properties;
using bank.utilities;
using XmlTransform;

namespace bank.reports
{
    public class Report
    {
        public string Template { get; set; }
        public bool DebugMode { get; set; } = false;
        public IList<FactColumn> Columns { get; internal set; } = new List<FactColumn>();

        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public List<ChartConfig> Charts { get; set; } = new List<ChartConfig>();
        public Dictionary<string, LineItem> _sections = new Dictionary<string, LineItem>();
        public string CurrentSection { get; set; }
        public bool IsCurrentPeriod { get; set; }
        private DateTime _maxPeriod;
        public DateTime MaxPeriod
        {
            get
            {
                return _maxPeriod = Columns[0].Facts.Max(x => x.Value.Period).Value;
            }
        }

        public ReportTypes ReportType
        {
            get
            {
                return (ReportTypes)Enum.Parse(typeof(ReportTypes), Template);
            }
        }

        public void AddColumn(FactColumn factColumn)
        {
            var columnIndex = Columns.Count;
            Columns.Add(factColumn);
        }

        private string _title;
        public string Title
        {
            get
            {
                if (_title == null && CurrentSectionLineItem.Type != null)
                {
                    _title = CurrentSectionLineItem.Label;
                }
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        public Fact GetFact(string mdrm, int column)
        {
            var resolver = new FactResolver();

            var concepts = resolver.ParseConcepts(mdrm);

            if (concepts.Count == 0)
            {

                var facts = Columns[column].Facts;

                if (facts.ContainsKey(mdrm))
                {
                    return facts[mdrm];
                }
            }
            else
            {
                var result = resolver.Evaluate(mdrm, Columns[column].Facts);
                var calculatedFact = new Fact
                {
                    Name = mdrm,
                    NumericValue = (decimal)(double)result
                };
                return calculatedFact;
            }
            return null;
        }

        private string GetResource(string key)
        {
            var resource = Path.Combine(Settings.ReportTemplatePath, key + ".xml");

            return resource;
        }

        public List<string> MdrmList(LineItem lineItem)
        {
            var list = new List<string>();
            MdrmListChildren(lineItem, list);
            return list;
        }

        private void MdrmListChildren(LineItem lineItem, List<string> list)
        {
            var resolver = new FactResolver();


            if (lineItem.Concepts.Any())
            {
                list.AddRange(lineItem.Concepts.Select(x => x.MDRM).ToArray());

                foreach (var concept in lineItem.Concepts)
                {
                    var resolved = resolver.ParseConcepts(concept.MDRM);
                    if (resolved.Any())
                    {
                        list.AddRange(resolved);
                    }
                }
            }

            if (lineItem.ChartConfig != null)
            {
                list.AddRange(lineItem.ChartConfig.Concepts.Select(x => x.MDRM));
            }

            foreach (var subline in lineItem.LineItems)
            {
                MdrmListChildren(subline, list);
            }
        }

        public LineItem CurrentSectionLineItem
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentSection))
                {
                    CurrentSection = Section.First().Key;
                }

                if (Section != null && Section.ContainsKey(CurrentSection))
                {
                    return Section[CurrentSection];
                }
                else
                {
                    return LineItems.First().LineItems.First();
                }
            }
        }

        //public List<string> MdrmList { get; internal set; } = new List<string>();
        public Dictionary<string, LineItem> Section
        {
            get
            {
                return _sections;
            }
        }


        private void ParseNode(XElement element, LineItem lineItem)
        {
            var childLineItem = new LineItem();
            var name = element.Attribute("name");
            var alias = element.Attribute("alias");
            var type = element.Attribute("type");
            var label = element.Attribute("label");
            var isAbstract = element.Attribute("abstract") != null ?
                                bool.Parse(element.Attribute("abstract").Value) :
                                true;

            var mdrm = !isAbstract ? (name != null ? name.Value : null) : null;

            if (!isAbstract && !string.IsNullOrWhiteSpace(mdrm))
            {
                lineItem.Concepts.Add(new Concept
                {
                    MDRM = mdrm
                });
                return;
            }

            if (name != null) { childLineItem.LineNumber = name.Value; }
            if (type != null) { childLineItem.Type = type.Value; }

            if (label != null) childLineItem.Label = label != null ? label.Value : null;

            if (type != null)
            {
                switch (type.Value.ToLower())
                {
                    case "section":
                        _sections.Add(alias.Value, childLineItem);
                        break;

                }
            }

            if (element.Name.LocalName == "chart")
            {
                ParseChartNode(element, lineItem);
            }

            //if (lineItem.ChartConfig == null)
            //{
                lineItem.LineItems.Add(childLineItem);

                foreach (var childElement in element.Elements())
                {
                    ParseNode(childElement, childLineItem);
                }
            //}

        }

        private void ParseChartNode(XElement element, LineItem lineItem)
        {
            var title = element.Attribute("title");
            var type = element.Attribute("type");

            var chartConfig = ChartConfig.Build(type.Value);

            chartConfig.Name = title.Value;
            chartConfig.Type = (SeriesType)Enum.Parse(typeof(SeriesType), type.Value, true);

            if (chartConfig.GetType() == typeof(SankeyChartConfig))
            {
                ParseSankeyChart(chartConfig as SankeyChartConfig, element, lineItem);
            }
            else
            {
                ParseChartConfig(chartConfig, element, lineItem);
            }

            lineItem.ChartConfig = chartConfig;

            Charts.Add(chartConfig);
            
        }

        private void ParseChartConfig(ChartConfig chartConfig, XElement element, LineItem lineItem)
        {
            var seriesElements = element.Descendants("series");

            foreach(var seriesElement in seriesElements)
            {
                var concept = seriesElement.Attribute("concept").Value;
                var seriesType = seriesElement.Attribute("type").Value;

                var series = new TrendSeries
                {
                    FactName = concept,
                    SeriesType = (SeriesType)Enum.Parse(typeof(SeriesType), seriesType, true)
                };

                chartConfig.Series.Add(series);
            }
        }

        private void ParseSankeyChart(SankeyChartConfig chartConfig, XElement element, LineItem lineItem)
        {
            
            var columnElements = element.Descendants("column");

            foreach (var columnElement in columnElements)
            {
                var column = new ChartColumnConfig
                {
                    Type = columnElement.Attribute("type").Value,
                    Id = columnElement.Attribute("id").Value,
                    Label = columnElement.Attribute("label").Value
                };
                chartConfig.Columns.Add(column);
            }

            var rowElements = element.Descendants("row");
            foreach (var rowElement in rowElements)
            {
                var values = rowElement.Descendants("value");
                var counter = 0;
                var row = new ChartRowConfig();

                foreach (var value in values)
                {
                    var columnConfig = chartConfig.Columns[counter++];
                    var isFormula = value.Attribute("isFormula");
                    var rowValue = new ChartRowValueConfig
                    {
                        ColumnConfig = columnConfig,
                        Text = value.Value,
                        IsFormula = isFormula != null ? bool.Parse(isFormula.Value) : false
                    };

                    row.Values.Add(rowValue);

                }

                chartConfig.Rows.Add(row);
            }
            lineItem.ChartConfig = chartConfig;
        }

        //public void LoadDefinitions()
        //{
        //    var list = MdrmList(CurrentSectionLineItem);

        //    var repo = new MdrmDefinitionRepository();
        //    var definitions = repo.Filter(list.ToArray());

        //    LoadDefinitions(CurrentSectionLineItem, definitions);

        //}

        //private void LoadDefinitions(LineItem lineitem, IList<MdrmDefinition> definitions)
        //{
        //    foreach(var concept in lineitem.Concepts)
        //    {
        //        concept.MdrmDefinition = definitions.SingleOrDefault(x => x.Mdrm == concept.MDRM);
        //    }
        //}

        public void Parse()
        {
            var path = GetResource(Template);
            var patch = path.Replace(".xml", ".patch.xml");

            var xmlText = File.ReadAllText(path);

            if (File.Exists(patch))
            {
                var patchText = File.ReadAllText(patch);

                var transformer = new XmlTransformer();
                var source = xmlText;
                var transform = patchText;

                var result = string.Empty;

                xmlText = transformer.ApplyTransform(source, transform);

            }

            var xml = XDocument.Parse(xmlText);

            var root = new LineItem();


            ParseNode(xml.Root.Elements().First(), root);

            this.LineItems = root.LineItems;

            if (!_sections.Any())
            {
                _sections = LineItems.First().LineItems.ToDictionary(x => x.LineNumber.ToLower(), x => x);
            }

        }

        //public void Parse()
        //{
        //    var path = GetResource(Template);

        //    var xml = XDocument.Load(path);
        //    var root = new LineItem();

        //    ParseNode(xml.Root, root);


        //    foreach (var line in lines)
        //    {
        //        if (Title == null)
        //        {
        //            Title = line;
        //            continue;
        //        }

        //        if (string.IsNullOrWhiteSpace(line)) continue;

        //        var items = line.Split('\t');
        //        var lineNumber = items[0].Trim();
        //        var textParts = items[1].Trim().Split('|');
        //        var text = textParts[0].Trim();
        //        var subTemplate = textParts.Length > 1 ? textParts[1].Trim() : string.Empty;
        //        var mdrm = items.Length >= 3 ? items[2].Trim() : string.Empty;

        //        var lineNumberParts = lineNumber.Split('.');

        //        var lineItem = new LineItem
        //        {
        //            LineNumber = lineNumberParts.Last(),
        //            MDRM = mdrm,
        //            Text = text,
        //            SubTemplate = !string.IsNullOrWhiteSpace(subTemplate) ? subTemplate : null,
        //            LineItems = new Dictionary<string, LineItem>()
        //        };

        //        MdrmList.Add(mdrm);

        //        var lineItems = LineItems;

        //        for (int i = 0; i < lineNumberParts.Length - 1; i++)
        //        {
        //            var key = lineNumberParts[i];

        //            if (lineItems.ContainsKey(key))
        //            {
        //                lineItems = lineItems[key].LineItems;
        //            }
        //        }

        //        lineItems.Add(lineItem.LineNumber, lineItem);
        //    }
        //}

    }
}
