using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Hanasu.Core.Stations;
using Hanasu.Core.Preprocessor;
using Hanasu.Core.Utilities;

namespace Hanasu.Core
{
    public static class GlobalHanasuCore
    {
        static GlobalHanasuCore()
        {
            if (Initialized == false)
                return;
        }

        private static Func<string, object,object> eventHandler = null;

        /// <summary>
        /// Initializes the Hanasu core.
        /// </summary>
        /// <param name="_eventHandler">Event handler for sending data to the GUI.</param>
        /// <param name="pluginDir">The directory used in searching for plugins.</param>
        public static void Initialize(Func<string, object, object> _eventHandler, string pluginDir)
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

            if (CurrentPlayer == null) return; //Somethings wrong

            var url = stat.DataSource;

            if (stat.StationType == StationType.Radio)
            {

                if (url.Segments.Last().Contains("."))
                {
                    var ext = url.Segments.Last();
                    ext = ext.Substring(ext.LastIndexOf("."));


                    if ((!CurrentPlayer.Supports(ext) && !HtmlTextUtility.ExtensionIsWebExtension(ext)) || (HtmlTextUtility.ExtensionIsWebExtension(ext) && stat.ExplicitExtension != null))
                    {
                        //pre-process the url here.
                        //stolen code from Hanasu 1.0 because it works like it should. :|
                        var pro = Preprocessor.PreprocessorService.GetProcessor(url, stat.ExplicitExtension);

                        if (pro.GetType().BaseType == typeof(Preprocessor.MultiStreamPreprocessor))
                        {
                            var p = (Preprocessor.MultiStreamPreprocessor)pro;

                            var entries = p.Parse(url);

                            if (entries.Length == 0)
                            {
                                return;
                            }
                            else if (entries.Length == 1)
                            {
                                url = new Uri(entries[0].File);
                            }
                            else
                            {
                                var result = (Tuple<bool, IMultiStreamEntry>)PushMessageToGUI(StationMultipleServersFound, entries);

                                if (result == null) return;

                                if (result.Item1 == false) return;
                            }
                        }

                        Preprocessor.PreprocessorService.Process(ref url);
                    }
                }
            }

            try
            {
                CurrentStation = stat;
                CurrentPlayer.Play(url, CurrentStation.StationType == StationType.Radio ? MediaType.Audio : MediaType.Video);
                PushMessageToGUI(NowPlayingReset, null);
                PushMessageToGUI(NowPlayingStatus, true);
            }
            catch (Exception)
            {
                PushMessageToGUI(StationConnectionError, null);
            }
        }

        public static Station CurrentStation { get; private set; }

        public const string StationsUpdated = "StationsUpdated";
        public const string SongTitleUpdated = "SongTitleUpdated";
        public const string StationTitleUpdated = "StationTitleUpdated";
        public const string NowPlayingReset = "NowPlayingReset";
        public const string NowPlayingStatus = "NowPlayingStatus";
        public const string StationConnectionError = "StationConnectionError";
        public const string StationMultipleServersFound = "StationMultipleServersFound";
        public const string MediaTypeDetected = "MediaTypeDetected";

        public static bool Initialized { get; private set; }

        public static PluginImporterInstance Plugins { get; private set; }
        public static Stations.StationsService StationsService { get; private set; }

        internal static IMediaPlayer CurrentPlayer { get; private set; }

        private static object PushMessageToGUI(string eventstr, object data)
        {
            if (eventHandler != null)
                return eventHandler(eventstr, data);

            return null;
        }

        public static void OnSongTitleDetected(IMediaPlayer player, string songdata)
        {
            PushMessageToGUI(SongTitleUpdated, songdata);
            return;
        }
        public static void OnStationTitleDetected(IMediaPlayer player, string stationdata)
        {
            PushMessageToGUI(StationTitleUpdated, stationdata);
            return;
        }

        public static void OnStationMediaTypeDetected(IMediaPlayer player, bool IsVideo)
        {
            PushMessageToGUI(MediaTypeDetected, IsVideo);
            return;
        }

        public static void StopStation()
        {
            CurrentPlayer.Stop();
            PushMessageToGUI(NowPlayingReset, null);
            PushMessageToGUI(NowPlayingStatus, false);
        }

        public static void OnStationConnectionTerminated(IMediaPlayer player)
        {
            //When the station stops the connection... not the user.
            PushMessageToGUI(NowPlayingReset, null);
            PushMessageToGUI(NowPlayingStatus, false);
        }

        public static int GetVolume()
        {
            if (CurrentPlayer == null)
                return 0;
            else
                return CurrentPlayer.Volume;
        }

        public static void SetVolume(int vol)
        {
            if (vol > 100 || vol < 0) throw new ArgumentOutOfRangeException("vol");

            if (CurrentPlayer != null)
                CurrentPlayer.Volume = vol;
        }

        public static object GetPlayerView()
        {
            if (CurrentPlayer != null)
                return CurrentPlayer.GetVideoControl();

            return null;
        }
    }
}
