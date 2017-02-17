using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using bank.reports;

namespace bank.web.models
{
    public class TemplateColumnViewModel
    {

        public TemplateColumn Col { get; set; }

        private bool? _isInCard = null;
        public bool IsInCard
        {
            get
            {
                if (_isInCard.HasValue)
                {
                    return _isInCard.Value;
                }

                return Parent.IsInCard;
            }
            set
            {
                _isInCard = value;
            }
        }

        public bool IsChild
        {
            get
            {
                return Parent.IsChild;
            }
        }
        public ColsViewModel Parent { get; set; }

        public string Partial(TemplateElement element)
        {
            var partial = element.Partial ?? element.GetType().Name;

            return string.Format("template/_{0}", partial);
        }

        //public TemplateColumnViewModel(TemplateColumn col)
        //{
        //    Col = col;
        //}

        public string ColumnGrid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Col.GridOverride))
                {
                    return Col.GridOverride;
                }

                var colSize = 12 / Parent.Columns.Count;
                var remainder = 12 % Parent.Columns.Count;

                if (remainder != 0 || IsInCard || Parent.IsInCard)
                {
                    return "col";
                }

                var xs = 12;
                var sm = 12;
                var md = 12;
                var lg = 12;
                var xl = 12;

                if (!Parent.IsChild)
                {
                    switch (colSize)
                    {
                        case 2:
                            sm = 6;
                            md = 6;
                            lg = 6;
                            xl = 6;
                            break;
                        case 3:
                            sm = 6;
                            md = 6;
                            lg = 3;
                            xl = 3;
                            break;
                        case 4:
                            sm = 12;
                            md = 12;
                            lg = 4;
                            xl = 4;
                            break;
                        case 6:
                            lg = 6;
                            xl = 6;
                            break;
                    }
                }

                return string.Format("col-xs-{0} col-sm-{1} col-md-{2} col-lg-{3} col-xl-{4}", xs, sm, md, lg, xl);
            }
        }


        public IList<TemplateElement> Elements
        {
            get
            {
                var elements = new List<TemplateElement>();

                if (!Col.Elements.Any())
                {
                    elements.Add(new EmptyElement());
                }

                elements.AddRange(Col.Elements);

                return elements;
            }
        }

        public MvcHtmlString ColumnGridCss
        {
            get
            {
                var css = Col.CssClasses ?? "col";

                if (!css.Contains("col"))
                {
                    css += " col";
                }

                return new MvcHtmlString(css);
            }
        }

        public MvcHtmlString ShowCard(bool isStart)
        {
            //var show = !IsChild && Elements.Where(x => x as EmptyElement == null).Any();
            var show = !Parent.IsInCard && Elements.Where(x => x as EmptyElement == null).Any();

            IsInCard = show;

            var snippet = isStart ? "<div class='card'>" : "</div>";
            return new MvcHtmlString(show ? snippet : "");

        }
        
        public MvcHtmlString CardBlock(TemplateElement element, bool isStart)
        {
            var table = element as TableElement;
            var html = element as HtmlElement;
            var empty = element as EmptyElement;
            var custom = element as TemplateElement != null
                && !string.IsNullOrWhiteSpace(element.Partial)
                && !string.IsNullOrWhiteSpace(element.DataSource);

            var suppress =  custom ||
                        table != null || 
                        empty != null ||
                        (html != null && !string.IsNullOrWhiteSpace(html.Content));

            var snippet = isStart ? "<div class='card-block'>" : "</div>";

            return new MvcHtmlString(suppress ? "" : snippet);

        }

    }
}