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

        public TableOrientation Orientation { get; internal set; }
        public int Level { get; internal set; }
        public bool ShowRank { get; internal set; }
        public bool Thousands { get; internal set; }

        public TableElement()
        {
        }

        private IList<decimal> _sum;
        public IList<decimal> Sum()
        {
            if (_sum == null)
            {
                var list = new List<decimal>();
                var columnIndex = 0;

                foreach (var column in DataColumns)
                {
                    decimal sum = 0;
                    foreach (var row in Rows)
                    {
                        var tableRowGroup = row as TableRowGroup;

                        if (tableRowGroup != null)
                        {
                            sum += tableRowGroup.Table.Sum()[columnIndex];
                            continue;
                        }

                        var tableRow = row as TableRow;
                        if (tableRow != null)
                        {
                            var fact = column.GetCell(tableRow.Concept);

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

                    columnIndex++;
                    //var format = string.Format("N{0}", sum > 1000 ? 0 : 2);
                    //list.Add(sum.ToString(format));
                    list.Add(sum);
                }

                _sum = list;
            }

            return _sum;
        }
    }
}