using System;
using System.Collections.Generic;

namespace bank.reports
{
    public class TableElement : TemplateElement, ITableRow
    {
        public List<ITableRow> Rows { get; set; } = new List<ITableRow>();

        public TableRowTypes TableRowType
        {
            get
            {
                return TableRowTypes.Table;
            }
            
        }

        public object Orientation { get; internal set; }
        public int Level { get; internal set; }

        public TableElement()
        {
        }

        public IList<string> Sum()
        {
            var list = new List<string>();

            foreach(var column in DataColumns)
            {
                decimal sum = 0;
                foreach(var row in Rows)
                {
                    var tableRowGroup = row as TableRowGroup;

                    if (tableRowGroup != null)
                    {
                        continue;
                    }

                    var tableRow = row as TableRow;
                    if (tableRow != null)
                    {
                        var fact = column.GetCell(tableRow.Concept, column);

                        if (fact != null && fact.NumericValue.HasValue)
                        {
                            if (!tableRow.Concept.Negative.HasValue || !tableRow.Concept.Negative.Value)
                            {
                                sum += fact.NumericValue.Value;
                            }
                            else
                            {
                                sum -= fact.NumericValue.Value;
                            }
                        }
                    }
                }

                var format = string.Format("N{0}", sum > 1000 ? 0 : 2);
                
                list.Add(sum.ToString(format));
            }

            return list;
        }
    }
}