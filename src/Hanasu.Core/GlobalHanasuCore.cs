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
using Hanasu.Core.Songs;
using Hanasu.Core.ArtistService;
using System.Timers;
using Hanasu.Core.Stations.Shoutcast;
using System.Threading.Tasks;
using System.Threading;

namespace Hanasu.Core
{
    public static class GlobalHanasuCore
    {
        static GlobalHanasuCore()
        {
            if (Initialized == false)
                return;
        }

        private static Func<string, object, object> eventHandler = null;

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

            playingTimer = new System.Timers.Timer(1000);
            playingTimer.Elapsed += new ElapsedEventHandler(playingTimer_Elapsed);

            nowPlayingTimer = new System.Timers.Timer(TimeSpan.FromMinutes(3.2).TotalMilliseconds);
            nowPlayingTimer.Elapsed += new ElapsedEventHandler(nowPlayingTimer_Elapsed);

            SongService = new Songs.SongService();

            ArtistService = new Core.ArtistService.ArtistService();

            System.Threading.Tasks.Parallel.Invoke(() =>
                {
                    StationsService.LoadStationsFromRepoAsync().ContinueWith(t =>
                        {
                            PushMessageToGUI(StationsUpdated, StationsService.Stations);

                            PollStationsNowPlaying();
                            nowPlayingTimer.Start();
                        });

                },
                () =>
                {
                    Plugins = new PluginImporterInstance();
                    try
                    {
                        if (!Directory.Exists(pluginDir))
                            Directory.CreateDirectory(pluginDir);

                        var ac = new AggregateCatalog();

                        ac.Catalogs.Add(new DirectoryCatalog(pluginDir, "*.dll"));

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
                        else
                        {
                            PushMessageToGUI(CoreWarningPushed, "Player components were not loaded. Hanasu will be unable to play stations.");
                        }
                    }
                    catch (Exception)
                    {
                    }
                });

            Initialized = true;
        }

        static void nowPlayingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PollStationsNowPlaying();
        }

        private static void PollStationsNowPlaying()
        {
            if (StationsService.Stations.Count == 0) return;

            int i = 0;
            while (i != StationsService.Stations.Count - 1)
            {
                var station = StationsService.Stations[i];

                try
                {
                    if (station.ServerType == StationServerType.Shoutcast)
                    {
                        if (station.Name == CurrentStation.Name)
                            station.DetectedNowPlaying = ShoutcastService.GetShoutcastStationCurrentSong(CurrentStation, CurrentPlayer.DataAttributes).ToSongString();
                        else
                        {
                            Uri url = station.DataSource;
                            if (station._directstreamurl == null)
                            {
                                PreprocessUrl(ref url, CurrentStation, null, true);
                                station._directstreamurl = url;
                            }
                            try
                            {
                                station.DetectedNowPlaying = ShoutcastService.GetShoutcastStationCurrentSong(station, station._directstreamurl != null ? station._directstreamurl.ToString() : url.ToString()).ToSongString();
                            }
                            catch (Exception)
                            {
                                station.DetectedNowPlaying = "N/A";
                            }
                        }
                    }

                    PushActionToGUIDispatcher(() =>
                        {
                            StationsService.Stations[i] = station;
                        });
                }
                catch (Exception)
                {
                }

                i++;
            }
            PushMessageToGUI(StationsUpdated, StationsService.Stations);
        }
        private static bool PreprocessUrl(ref Uri url, Station stat, string ext, bool autochoose = false)
        {
            var pro = Preprocessor.PreprocessorService.GetProcessor(url, stat.ExplicitExtension);

            if (pro == null && stat.ExplicitExtension == null)
            {
                //We don't know if its possible to play
                return false;
            }
            else
            {
                //We know its possible to play and have a preprocessor for it.

                if (pro.GetType().BaseType == typeof(Preprocessor.MultiStreamPreprocessor))
                {
                    var p = (Preprocessor.MultiStreamPreprocessor)pro;

                    var entries = p.Parse(url);

                    if (entries.Length == 0)
                    {
                        return false;
                    }
                    else if (entries.Length == 1)
                    {
                        url = new Uri(entries[0].File);
                    }
                    else
                    {
                        if (autochoose == false)
                        {
                            var result = (Tuple<bool, IMultiStreamEntry>)PushMessageToGUI(StationMultipleServersFound, new Tuple<Station, IMultiStreamEntry[]>(stat, entries));

                            if (result == null) return false;

                            if (result.Item1 == false) return false;

                            url = new Uri(result.Item2.File);
                        }
                        else
                        {
                            url = new Uri(entries[0].File);

                            return true;
                        }
                    }
                }

                Preprocessor.PreprocessorService.Process(ref url);
                return true;
            }
        }

        static void playingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CurrentPlayer != null)
            {
                PushActionToGUIDispatcher(() =>
                    {
                        var pos = CurrentPlayer.GetPosition();
                    });
            }
        }

        private static System.Timers.Timer playingTimer = null;
        private static System.Timers.Timer nowPlayingTimer = null;

        public static void PlayStation(Nullable<Station> station = null)
        {
            //deal with finding the direct url here.

            if (CurrentPlayer == null) return; //Somethings wrong

            Station stat = station.HasValue ? station.Value : CurrentStation;

            if (CurrentStation == null)
                throw new Exception();

            var url = stat.DataSource;

            try
            {
                if (stat.StationType == StationType.Radio)
                {

                    if (url.Segments.Last().Contains("."))
                    {
                        var ext = url.Segments.Last();
                        ext = ext.Substring(ext.LastIndexOf("."));


                        if ((!CurrentPlayer.Supports(ext)))
                        {
                            if (stat.ExplicitExtension != null)
                            {
                                if (!CurrentPlayer.Supports(stat.ExplicitExtension))
                                    if (!PreprocessUrl(ref url, stat, ext))
                                        return;
                            }
                            else
                                if (!PreprocessUrl(ref url, stat, ext))
                                    return;
                        }

                    }
                }

                if (playingTimer.Enabled) playingTimer.Stop();

                CurrentStation = stat;

                CurrentPlayer.Volume = GetVolume();
                CurrentPlayer.Play(url, CurrentStation.StationType == StationType.Radio ? MediaType.Audio : MediaType.Video);

                playingTimer.Start();

                PushMessageToGUI(NowPlayingReset, null);
                PushMessageToGUI(NowPlayingStatus, true);

                if (CurrentStation.ServerType != StationServerType.Auto && CurrentStation.ServerType != StationServerType.None && CurrentStation.ServerType != StationServerType.Unknown)
                {
                    Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(5000);

                            if (CurrentPlayer.DataAttributes.ContainsKey("SourceURL"))
                            {

                                var urlx = CurrentPlayer.DataAttributes["SourceURL"].ToString();

                                OnStationTypeDetected(CurrentPlayer,
                                    new Tuple<PlayerDetectedStationType, Uri>(CurrentStation.ServerType == StationServerType.Shoutcast ? PlayerDetectedStationType.Shoutcast : PlayerDetectedStationType.Unknown,
                                        new Uri(urlx)));
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                PushMessageToGUI(StationConnectionError, ex);
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
        public const string PlayerDetectedStationTypeDetected = "PlayerDetectedStationTypeDetected";
        public const string SongCaughtAtBeginning = "SongCaughtAtBeginning";
        public const string StationMessagePushed = "StationMessagePushed";
        public const string CoreWarningPushed = "CoreWarningPushed";
        public const string CoreDispatcherInvoke = "CoreDispatcherInvoke";
        public const string StationBufferingStatusChanged = "StationBufferingStatusChanged";

        public static bool Initialized { get; private set; }

        public static PluginImporterInstance Plugins { get; private set; }
        internal static Stations.StationsService StationsService { get; private set; }
        internal static SongService SongService { get; set; }
        internal static Hanasu.Core.ArtistService.ArtistService ArtistService { get; set; }

        internal static IMediaPlayer CurrentPlayer { get; private set; }

        public static Songs.SongData CurrentSong { get; private set; }

        private static object PushMessageToGUI(string eventstr, object data)
        {
            if (eventHandler != null)
                return eventHandler(eventstr, data);

            return null;
        }

        private static object PushActionToGUIDispatcher(Action a)
        {
            if (eventHandler != null)
                return eventHandler(CoreDispatcherInvoke, a);

            return null;
        }

        public static void OnBufferingStatusChanged(IMediaPlayer player, bool status)
        {
            PushMessageToGUI(StationBufferingStatusChanged, status);
        }

        public static void OnSongTitleDetected(IMediaPlayer player, string songdata)
        {
            if (CurrentSong != null)
                CurrentSong = SongService.ParseSongData(songdata, CurrentStation);
            else
            {
                var x = SongService.ParseSongData(songdata, CurrentStation);

                if (CurrentSong.Artist == x.Artist && CurrentSong.TrackTitle == x.TrackTitle)
                    return;
            }
            PushMessageToGUI(SongTitleUpdated, songdata);

            PushActionToGUIDispatcher(() =>
                        {
                            var station = CurrentStation;
                            station.DetectedNowPlaying = songdata;

                            int i = 0;
                            foreach (Station sv in StationsService.Stations)
                            {
                                if (sv.Name == station.Name) break;
                                i++;
                            }

                            StationsService.Stations[i] = station;
                        });
            PushMessageToGUI(StationsUpdated, StationsService.Stations);

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
            if (CurrentPlayer == null) return;

            CurrentPlayer.Stop();
            PushMessageToGUI(NowPlayingReset, null);
            PushMessageToGUI(NowPlayingStatus, false);

            CurrentSong = new Songs.SongData();
            playingTimer.Stop();
        }

        public static void OnStationConnectionTerminated(IMediaPlayer player)
        {
            //When the station stops the connection... not the user.
            PushMessageToGUI(NowPlayingReset, null);
            PushMessageToGUI(NowPlayingStatus, false);

            CurrentSong = new Songs.SongData();

            playingTimer.Stop();
        }
        #region No Idea why i did this
        public static int Volume
        {
            get { return GetVolume(); }
            set { SetVolume(value); }
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
        #endregion

        public static bool IsMuted
        {
            get
            {
                if (CurrentPlayer != null)
                    return CurrentPlayer.IsMuted;
                else
                    return false;
            }
            set { CurrentPlayer.IsMuted = value; }
        }

        public static object GetPlayerView()
        {
            if (CurrentPlayer != null)
                return CurrentPlayer.GetVideoControl();

            return null;
        }

        public static SongData GetExtendedSongInfoFromCurrentSong()
        {
            if (CurrentSong.TrackTitle == null)
                throw new NullReferenceException("CurrentSong is null!");

            var x = CurrentSong;
            if (SongService.DataSource.GetAlbumInfo(ref x))
                CurrentSong = x;

            return x;
        }

        public static ArtistInfo GetArtistInfoFromCurrentSong()
        {
            return ArtistService.FindArtist(CurrentSong);
        }

        public static SongData GetLyricsFromCurrentSong()
        {
            if (CurrentSong.TrackTitle == null)
                throw new NullReferenceException("CurrentSong is null!");

            var x = CurrentSong;
            if (SongService.GetLyricsFromSong(ref x))
                CurrentSong = x;

            return x;
        }

        public static void OnStationTypeDetected(IMediaPlayer player, Tuple<PlayerDetectedStationType, Uri> playerDetectedStationType)
        {
            PushMessageToGUI(PlayerDetectedStationTypeDetected, playerDetectedStationType);
            return;
        }

        public static void OnStationMessage(IMediaPlayer player, string name)
        {
            PushMessageToGUI(StationMessagePushed, name);
            return;
        }
    }

    public enum PlayerDetectedStationType
    {
        None = 0,
        Unknown = 1,
        Shoutcast = 2
    }
}
