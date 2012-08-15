using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Hanasu.Core.Stations;

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

        /// <summary>
        /// Initializes the Hanasu core.
        /// </summary>
        /// <param name="_eventHandler">Event handler for sending data to the GUI.</param>
        /// <param name="pluginDir">The directory used in searching for plugins.</param>
        public static void Initialize(Action<string, object> _eventHandler, string pluginDir)
        {
            if (Initialized) return;

            eventHandler = _eventHandler;

            StationsService = new Stations.StationsService();

            StationsService.LoadStationsFromRepoAsync().ContinueWith(t =>
                {
                    PushMessageToGUI(StationsUpdated, StationsService.Stations);
                });


            Plugins = new PluginImporterInstance();

            try
            {
                if (!Directory.Exists(pluginDir))
                    Directory.CreateDirectory(pluginDir);

                var ac = new AggregateCatalog();

                ac.Catalogs.Add(new DirectoryCatalog(pluginDir,"*.dll"));

                foreach (var dir in Directory.EnumerateDirectories(pluginDir))
                {
                    ac.Catalogs.Add(new DirectoryCatalog(dir, "*.dll"));
                }

                var comp = new CompositionContainer(ac);
                comp.ComposeParts(Plugins);

                if (Plugins.Players.Count() > 0)
                {
                    CurrentPlayer = Plugins.Players.First();
                    CurrentPlayer.Initialize();
                }
            }
            catch (Exception)
            {
            }

            Initialized = true;
        }

        public static void PlayStation(Station stat)
        {
            //deal with finding the direct url here.

            CurrentStation = stat;
            CurrentPlayer.Play(stat.DataSource);
        }

        public static Station CurrentStation { get; private set; }

        public const string StationsUpdated = "StationsUpdated";

        public static bool Initialized { get; private set; }

        public static PluginImporterInstance Plugins { get; private set; }
        public static Stations.StationsService StationsService { get; private set; }

        internal static IMediaPlayer CurrentPlayer { get; private set; }

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
