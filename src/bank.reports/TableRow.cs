using System;
using System.Collections.Generic;
using bank.poco;


namespace bank.reports
{
    public class TableRow : ITableRow
    {
        public Concept Concept { get; internal set; }

        public TableRowTypes TableRowType { get; set; }

        private string _label;
        public string Label
        {

            get
            {
                if (_label == null && Concept != null)
                {
                    return Concept.Label;
                }
                return _label;
            }
            set
            {
                _label = value;
            }

        }

        private bool? _link = null;
        public bool Link
        {
            get
            {
                if (_link.HasValue)
                {
                    return _link.Value;
                }
                else
                {
                    return Concept.ConceptKeys.Count == 1;
                }

            }
            set
            {
                _link = value;
            }
        }

        public string SubText { get; internal set; }
    }
}
