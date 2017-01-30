using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bank.poco;
using bank.reports;

namespace bank.web.models
{
    public interface IReports
    {
        IList<Report> Reports { get; }

        //IList<Column> Columns { get; }

        Organization Organization { get; set; }
    }
}