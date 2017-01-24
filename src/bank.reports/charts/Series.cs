﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bank.reports.charts
{

    public class Series
    {
        public SeriesTypes Type { get; set; }
        public Concept Concept {

            get
            {
                return Concepts.First();
            }
        }
        public IList<Concept> Concepts { get; set; } = new List<Concept>();

        public Series(SeriesTypes type)
        {
            Type = type;
        }

        public SeriesData GetSeriesData(ChartConfig chart, Column column, Series series)
        {
            SeriesData seriesData;
            switch (Type)
            {
                case SeriesTypes.Pie:
                    seriesData = new PieSeriesData();
                    break;
                case SeriesTypes.Line:
                case SeriesTypes.Sparkline:
                case SeriesTypes.Spline:
                    seriesData = new LineSeriesData();
                    seriesData.SeriesType = Type;
                    break;
                case SeriesTypes.AreaSpline:
                    seriesData = new LineSeriesData();
                    seriesData.SeriesType = SeriesTypes.AreaSpline;
                    break;
                default:
                    throw new Exception("Series type not supported");
            }
            seriesData.Chart = chart;
            seriesData.Column = column;
            seriesData.Series = series;
            seriesData.Init();

            return seriesData;
        }

        //public static Series Build(SeriesTypes type)
        //{
        //    Series series;

        //    switch (type)
        //    {
        //        case SeriesTypes.Pie:
        //            series = new PieSeries();
        //            break;
        //        case SeriesTypes.Line:
        //            series = new LineSeries();
        //            break;
        //        default:
        //            throw new Exception("Series type not supported");
        //    }

        //    return series;
        //}

        //public abstract SeriesData GetSeriesData();
    }
}
