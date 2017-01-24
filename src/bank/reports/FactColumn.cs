using System;
using System.Collections.Generic;
using bank.poco;

namespace bank.reports
{
    public class FactColumn
    {
        public virtual string Header { get; }
        public virtual string HeaderUrl { get; set; }
        public FactColumnType ColumnType { get; set; }
        public Dictionary<string, Fact> Facts { get; set; }
        
        public FactColumn(FactColumnType columnType)
        {
            ColumnType = columnType;
        }
    }
}