using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;
using bank.utilities;

namespace bank.web.models
{
    public class ChartsSectionViewModel
    {
        public Report Report { get; set; }
        public LineItem SubSection { get; set; }

        public List<List<object>> Data
        {
            get
            {

                if (SubSection.ChartConfig != null)
                {
                    var data = new List<List<object>>();

                    var chartConfig = SubSection.ChartConfig as SankeyChartConfig;

                    var datum = new List<object>();

                    foreach (var columnConfig in chartConfig.Columns)
                    {

                        datum.Add(new
                        {
                            type = columnConfig.Type,
                            label = columnConfig.Label

                        });

                    }

                    data.Add(datum);
                    datum = new List<object>();
                    var resolver = new FactResolver();

                    foreach (var rowConfig in chartConfig.Rows)
                    {
                        bool addRow = true;
                        foreach(var value in rowConfig.Values)
                        {
                            var replaced = resolver.Evaluate(value.Text, Report.Columns[0].Facts);

                            double d;
                            if (double.TryParse(replaced.ToString(), out d))
                            {
                                if (d == 0)
                                {
                                    addRow = false;
                                    break;
                                }
                                datum.Add(d);
                            }
                            else
                            {
                                datum.Add(replaced);
                            }
                        }

                        if (addRow)
                        {
                            data.Add(datum);
                        }
                        datum = new List<object>();
                    }

                    return data;

                }

                return null;

            }
        }
    }

}