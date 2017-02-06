using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.reports;

namespace bank.web.models
{
    public class ColsViewModel
    {
        public IList<TemplateColumn> Columns { get; set; }
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
        public RowViewModel Parent { get; set; }


    }
}