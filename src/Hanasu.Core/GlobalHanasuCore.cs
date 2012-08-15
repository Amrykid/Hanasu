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
                Initialize();
        }

        public static void Initialize()
        {
            if (Initialized) return;

            StationsService = new Stations.StationsService();

            Initialized = true;
        }

        public static bool Initialized { get; private set; }

        public static Stations.StationsService StationsService { get; private set; }

        public static void OnSongTitleDetected(IMediaPlayer player, string songdata)
        {
        }
        public static void OnStationTitleDetected(IMediaPlayer player, string stationdata)
        {
        }
    }
}
