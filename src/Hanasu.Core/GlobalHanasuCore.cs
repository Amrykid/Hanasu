using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;

namespace Hanasu.Core
{
    public static class GlobalHanasuCore
    {
        static GlobalHanasuCore()
        {
            if (Initialized == false)
                //Initialize(null);
                return;
        }

        private static Action<string, object> eventHandler = null;

        public static void Initialize(Action<string, object> _eventHandler)
        {
            if (Initialized) return;

            eventHandler = _eventHandler;

            StationsService = new Stations.StationsService();

            StationsService.LoadStationsFromRepoAsync().ContinueWith(t =>
                {
                    PushMessageToGUI(StationsUpdated, StationsService.Stations);
                });



            Initialized = true;
        }

        public const string StationsUpdated = "StationsUpdated";

        public static bool Initialized { get; private set; }

        public static Stations.StationsService StationsService { get; private set; }

        private static void PushMessageToGUI(string eventstr, object data)
        {
            if (eventHandler != null)
                eventHandler(eventstr, data);
        }

        public static void OnSongTitleDetected(IMediaPlayer player, string songdata)
        {
        }
        public static void OnStationTitleDetected(IMediaPlayer player, string stationdata)
        {
        }
    }
}
