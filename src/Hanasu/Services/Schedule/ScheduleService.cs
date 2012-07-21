using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Windows.Controls;
using Hanasu.Services.Stations;
using Hanasu.Core;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Hanasu.Services.Schedule
{
    public class ScheduleService : BaseINPC, IStaticService
    {
        static ScheduleService()
        {
            if (Instance == null)
                Initialize();
        }
        public static void Initialize()
        {
            if (Instance == null)
                Instance = new ScheduleService();
        }
        public static ScheduleService Instance { get; private set; }

        public bool StationHasSchedule(Station station)
        {
            return station.ScheduleUrl != null && station.ScheduleType != StationScheduleType.none;
        }
        public object GetSuitableViewingControl(Stations.Station station)
        {
            switch (station.ScheduleType)
            {
                case StationScheduleType.ics:
                    {
                        //var r = Hanasu.Services.Schedule.Parsers.ICSParser.LoadFromUrl(station.ScheduleUrl);

                        /*var c = new Calendar();
                        c.SelectionMode = CalendarSelectionMode.MultipleRange;
                        foreach (var e in r.Events)
                            c.BlackoutDates.Add(new CalendarDateRange(e.DateStart, e.DateEnd));

                        return c; */

                        //Uses instant-cal to make a calendar widget

                        WebBrowser wb = new WebBrowser();
                        wb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        wb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

                        var str = "<iframe id='cv_if5' src='http://cdn.instantcal.com/cvir.html?id=cv_nav5&file=";
                        str += HtmlTextUtility.UrlEncode(station.ScheduleUrl.ToString()) + "&theme=BL&ccolor=%23ffffc0&dims=1&gtype=cv_monthgrid&gcloseable=0&gnavigable=1&gperiod=month&itype=cv_simpleevent\' allowTransparency=true scrolling='no' frameborder=0 height=600 width=800></iframe>";
                        wb.NavigateToString(str);
                        return wb;
                    }
                case StationScheduleType.image:
                    {
                        Image i = new Image();
                        i.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        i.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        i.Stretch = System.Windows.Media.Stretch.Uniform;
                        i.Source = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(station.ScheduleUrl, null, null, null);

                        return i;
                    }
                case StationScheduleType.page:
                    {
                        var html = HtmlTextUtility.GetHtmlFromUrl(station.ScheduleUrl);
                        html = Regex.Replace(html, "<a href=\".+?\">", "<a href=\"javascript:return false\">");

                        WebBrowser wb = new WebBrowser();
                        wb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        wb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        wb.ObjectForScripting = false;
                        wb.NavigateToString(html);
                        wb.HideScriptErrors(true);
                        return wb;
                    }
                default:
                    return null;
            }
        }
    }
}
