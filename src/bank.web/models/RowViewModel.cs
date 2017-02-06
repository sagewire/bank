using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class RowViewModel
    {
        public TemplateRow Row { get; set; }
        public bool IsChild
        {
            get
            {
                return Parent.IsChild;
            }
        }
        public bool IsInCard
        {
            get
            {
                return Parent.IsInCard;
            }
        }
        public RowsViewModel Parent { get; set; }

    }
}