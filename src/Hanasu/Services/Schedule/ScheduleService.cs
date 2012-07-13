using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Windows.Controls;
using Hanasu.Services.Stations;

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
        public Control GetSuitableViewingControl(Stations.Station station)
        {
            Control c = null;
            switch (station.ScheduleType)
            {
                case StationScheduleType.ics:
                    {
                        var r = Hanasu.Services.Schedule.Parsers.ICSParser.LoadFromUrl(station.ScheduleUrl);
                        break;
                    }
            }

            return c;
        }
    }
}
