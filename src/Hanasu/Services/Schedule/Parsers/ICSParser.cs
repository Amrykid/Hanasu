using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Hanasu.Services.Schedule.Parsers
{
    public class ICSParser
    {
        public static ICSCalendar LoadFromUrl(Uri url)
        {
            ICSCalendar res; 

            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(
                    url);
                res = Parse(data);
            }

            return res;
        }
        public static ICSCalendar Parse(string data)
        {
            var cal = new ICSCalendar();

            var bits = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            object currentObject = cal;
            ICSCalendarEvent currentEvent = new ICSCalendarEvent();
            string lastEventProperty = null;
            ICSCalendarTimeZone currentTimeZone = new ICSCalendarTimeZone();
            bool currentTimeZoneIsDayLight = false;

            List<ICSCalendarEvent> events = new List<ICSCalendarEvent>();
            int i = 0;
            while (i < bits.Length - 1)
            {
                try
                {
                    if (i == 0)
                    {
                        if (bits[0].ToUpper() != "BEGIN:VCALENDAR")
                            throw new Exception("Invalid calendar.");
                    }
                    else
                    {
                        var piece = bits[i].Split(new char[] { ':', ';' }, 2);
                        var name = piece[0];
                        var value = piece.Length > 1 ? piece[1] : bits[i];

                        if (currentObject is ICSCalendar)
                        {
                            #region Calendar
                            switch (name)
                            {
                                case "PRODID": cal.PRODID = value;
                                    break;
                                case "VERSION": cal.Version = value;
                                    break;
                                case "CALSCALE": cal.CalScale = value;
                                    break;
                                case "METHOD": cal.Method = value;
                                    break;
                                case "X-WR-CALNAME": cal.CalendarName = value;
                                    break;
                                case "X-WR-TIMEZONE": cal.CalendarTimeZone = value;
                                    break;
                                case "BEGIN":
                                    {
                                        switch (value)
                                        {
                                            case "VTIMEZONE":
                                                {
                                                    currentTimeZone = new ICSCalendarTimeZone();
                                                    currentObject = currentTimeZone;
                                                    break;
                                                }
                                            case "VEVENT":
                                                {
                                                    currentEvent = new ICSCalendarEvent();
                                                    currentObject = currentEvent;
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                default:
                                    break;
                            }
                            #endregion
                        }
                        else if (currentObject is ICSCalendarTimeZone)
                        {
                            #region TimeZone
                            switch (name)
                            {
                                case "TZID": currentTimeZone.TimeZoneID = value;
                                    break;
                                case "X-LIC-LOCATION":
                                    break;
                                case "BEGIN":
                                    {
                                        switch (value)
                                        {
                                            case "DAYLIGHT":
                                                currentTimeZoneIsDayLight = true;
                                                break;
                                            case "STANDARD":
                                                currentTimeZoneIsDayLight = false;
                                                break;
                                        }
                                        break;
                                    }
                                case "END":
                                    {
                                        switch (value)
                                        {
                                            case "DAYLIGHT":
                                                if (currentTimeZoneIsDayLight == true)
                                                {
                                                    cal.DayLightTimeZone = currentTimeZone;

                                                    if (bits[i + 1].StartsWith("BEGIN:STANDARD") || bits[i + 1].StartsWith("BEGIN:DAYLIGHT"))
                                                    {
                                                        currentTimeZone = new ICSCalendarTimeZone();
                                                        currentObject = currentTimeZone;
                                                    }
                                                    else
                                                        currentObject = cal;
                                                }
                                                break;
                                            case "STANDARD":
                                                if (currentTimeZoneIsDayLight == false)
                                                {
                                                    cal.StandardTimeZone = currentTimeZone;

                                                    if (bits[i + 1].StartsWith("BEGIN:STANDARD") || bits[i + 1].StartsWith("BEGIN:DAYLIGHT"))
                                                    {
                                                        currentTimeZone = new ICSCalendarTimeZone();
                                                        currentObject = currentTimeZone;
                                                    }
                                                    else
                                                        currentObject = cal;
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case "TZOFFSETFROM": currentTimeZone.TimeZoneOffsetFrom = value;
                                    break;
                                case "TZOFFSETTO": currentTimeZone.TimeZoneOffsetTo = value;
                                    break;
                                case "TZNAME": currentTimeZone.TimeZoneName = value;
                                    break;
                                case "DTSTART": currentTimeZone.DTStart = value;
                                    break;
                                case "RRULE": currentTimeZone.RepeatRule = value;
                                    break;
                                default:
                                    break;
                            }
                            #endregion
                        }
                        else if (currentObject is ICSCalendarEvent)
                        {
                            switch (name)
                            {
                                case "DTSTART": currentEvent.DateStart = DateTime.ParseExact(value.Replace("T", "").Replace("Z", ""), "yyyyMMddhhmmss", null);
                                    lastEventProperty = name;
                                    break;
                                case "DTEND": currentEvent.DateEnd = DateTime.ParseExact(value.Replace("T", "").Replace("Z", ""), "yyyyMMddhhmmss", null);
                                    lastEventProperty = name;
                                    break;
                                case "DTSTAMP": currentEvent.DateStamp = value;
                                    lastEventProperty = name;
                                    break;
                                case "UID": currentEvent.UID = value;
                                    lastEventProperty = name;
                                    break;
                                case "CREATED": currentEvent.CreatedDate = value;
                                    lastEventProperty = name;
                                    break;
                                case "DESCRIPTION": currentEvent.Description = value;
                                    lastEventProperty = name;
                                    break;
                                case "LAST-MODIFIED": currentEvent.LastModified = value;
                                    lastEventProperty = name;
                                    break;
                                case "LOCATION": currentEvent.Location = value;
                                    lastEventProperty = name;
                                    break;
                                case "SEQUENCE": currentEvent.Sequence = value;
                                    lastEventProperty = name;
                                    break;
                                case "STATUS": currentEvent.Status = (ICSCalendarEventStatus)Enum.Parse(typeof(ICSCalendarEventStatus), value);
                                    lastEventProperty = name;
                                    break;
                                case "SUMMARY": currentEvent.Summary = value;
                                    lastEventProperty = name;
                                    break;
                                case "TRANSP": currentEvent.TRANSP = value;
                                    lastEventProperty = name;
                                    break;
                                case "CLASS": currentEvent.Class = value;
                                    lastEventProperty = name;
                                    break;
                                case "RRULE": currentEvent.RepeatRule = value;
                                    lastEventProperty = name;
                                    break;
                                case "END":
                                    {
                                        if (value == "VEVENT")
                                        {
                                            events.Add(currentEvent);

                                            if (bits[i + 1].StartsWith("BEGIN:EVENT"))
                                            {
                                                currentEvent = new ICSCalendarEvent();
                                                currentObject = currentEvent;
                                            }
                                            else
                                                currentObject = cal;
                                        }
                                        break;
                                    }
                                default:
                                    if (lastEventProperty == "DESCRIPTION")
                                        currentEvent.Description = currentEvent.Description + value;
                                    break;
                            }
                        }

                    }
                }
                catch (Exception)
                {
                }

                i++;
            }

            cal.Events = events.ToArray();
            return cal;
        }
    }
    public struct ICSCalendar
    {
        public ICSCalendarTimeZone DayLightTimeZone { get; set; }
        public ICSCalendarTimeZone StandardTimeZone { get; set; }
        public string PRODID { get; set; }
        public string Version { get; set; }
        public ICSCalendarEvent[] Events { get; set; }
        public string CalScale { get; set; }
        public string Method { get; set; }
        public string CalendarName { get; set; }
        public string CalendarTimeZone { get; set; }
    }
    public struct ICSCalendarTimeZone
    {
        public string TimeZoneID { get; set; }
        public object TimeZoneOffsetFrom { get; set; }
        public object TimeZoneOffsetTo { get; set; }
        public object TimeZoneName { get; set; }
        public object DTStart { get; set; }
        public object RepeatRule { get; set; }
    }
    public struct ICSCalendarEvent
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string DateStamp { get; set; }
        public string UID { get; set; }
        public object CreatedDate { get; set; }
        public string Description { get; set; }
        public object LastModified { get; set; }
        public string Location { get; set; }
        public object Sequence { get; set; }
        public ICSCalendarEventStatus Status { get; set; }
        public string Summary { get; set; }
        public object TRANSP { get; set; }
        public string Class { get; set; }
        public string RepeatRule { get; set; }
    }

    public enum ICSCalendarEventStatus
    {
        CANCELLED = 0,
        CONFIRMED = 1,
    }
}
