using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class RowsViewModel
    {
        public IList<TemplateRow> Rows { get; set; }
        public bool IsChild
        {
            get
            {
                return Parent != null;
            }
        }

        public bool IsInCard
        {
            get
            {
                return Parent != null && Parent.IsInCard; ;
            }
        }
        
        public TemplateColumnViewModel Parent { get; set; }
    }
}