using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Messaging;
using Hanasu.Core;
using System.Threading.Tasks;
using Crystal.Command;
using Hanasu.Core.ArtistService;
using Crystal.Localization;

namespace Hanasu.ViewModel
{
    public class MainWindowNowPlayingViewModel : BaseViewModel
    {
        public MainWindowNowPlayingViewModel()
        {
            FindArtistInfoCommand = CommandManager.CreateCommandFromPropertyChangedAll((s, e) => CurrentArtistFromPlayer != null && IsFetchingArtistInfo == false && CurrentArtistInfoPaneIsOpen == false, (o) =>
                {
                    IsFetchingArtistInfo = true;
                    Task.Factory.StartNew(() => GlobalHanasuCore.GetArtistInfoFromCurrentSong())
                        .ContinueWith(t =>
                            {
                                if (t.Exception != null)
                                {
                                    //not found for some reason
                                }
                                else
                                {
                                    //found!

                                    CurrentArtistInfo = t.Result;

                                    Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                                        {
                                            CurrentArtistInfoPaneIsOpen = true;
                                        }));
                                }

                                IsFetchingArtistInfo = false;

                                t.Dispose();
                            });
                });

            FindLyricsCommand = CommandManager.CreateCommandFromBinding("LyricsPaneIsOpen", (s, e) => LyricsPaneIsOpen == false, (o) =>
                {
                    Task.Factory.StartNew(() => GlobalHanasuCore.GetLyricsFromCurrentSong())
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                //not found for some reason
                            }
                            else
                            {
                                //found!

                                if (GlobalHanasuCore.CurrentSong.Lyrics == null)
                                {
                                    Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                                    {
                                        LyricsPaneIsOpen = false;
                                    }));

                                    return;
                                }

                                Messenger.PushMessage(this, "LyricsReceived", GlobalHanasuCore.CurrentSong);

                                Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                                {
                                    LyricsPaneIsOpen = true;
                                }));
                            }

                            t.Dispose();
                        });

                    LyricsPaneIsOpen = true;
                });

            CurrentArtistInfoPaneIsOpen = true;
            CurrentArtistInfoPaneIsOpen = false;

            LyricsPaneIsOpen = false;

            HasArtist = false;

            IsFetchingArtistInfo = false;

            CurrentSongWasCaughtAtBeginning = false;
        }

        public CrystalCommand FindLyricsCommand { get; set; }


        public bool IsFetchingArtistInfo
        {
            get { return (bool)this.GetProperty("IsFetchingArtistInfo"); }
            set { this.SetProperty("IsFetchingArtistInfo", value); }
        }
        public CrystalCommand FindArtistInfoCommand { get; set; }

        [MessageHandler("StationTitleUpdated")]
        public void HandleStationTitleUpdated(object data)
        {
            StationTitleFromPlayer = (string)data;
        }

        [MessageHandler("PlayerDetectedStationTypeDetected")]
        public void HandlePlayerDetectedStationTypeDetected(object data)
        {
            Tuple<PlayerDetectedStationType, Uri> dat = (Tuple<PlayerDetectedStationType, Uri>)data;
            StationType = dat.Item1;
            DirectStationUrl = dat.Item2;
        }

        public bool CurrentSongWasCaughtAtBeginning { get; set; }

        public Uri DirectStationUrl { get; set; }
        public PlayerDetectedStationType StationType { get; set; }

        [MessageHandler("NowPlayingReset")]
        public void HandleNowPlayingReset(object data)
        {
            StationTitleFromPlayer = null;
            SongTitleFromPlayer = null;
            CurrentArtistFromPlayer = null;
            CurrentSongFromPlayer = null;
            CurrentArtistInfo = null;

            CurrentArtistInfoPaneIsOpen = false;
            LyricsPaneIsOpen = false;

            NowPlayingImage = null;

            CurrentSongWasCaughtAtBeginning = false;
        }

        [MessageHandler("SongTitleUpdated")]
        public void HandleSongTitleUpdated(object data)
        {
            SongTitleFromPlayer = (string)data;

            if (GlobalHanasuCore.CurrentSong.Artist != GlobalHanasuCore.CurrentStation.Name)
            {
                CurrentArtistFromPlayer = GlobalHanasuCore.CurrentSong.Artist;
                CurrentSongFromPlayer = GlobalHanasuCore.CurrentSong.TrackTitle;

                CurrentSongWasCaughtAtBeginning = false;

                CheckIfSongWasCaughtAtTheBeginning().ContinueWith((t) =>
                    {
                        HandleNowPlaying();
                    });
            }
        }

        private Task HandleNowPlaying()
        {
            return Task.Factory.StartNew(() => GlobalHanasuCore.GetExtendedSongInfoFromCurrentSong()).ContinueWith(x =>
            {

                Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                {
                    if (GlobalHanasuCore.CurrentSong.AlbumCoverUri != null)
                        NowPlayingImage = GlobalHanasuCore.CurrentSong.AlbumCoverUri;
                    else
                        if (GlobalHanasuCore.CurrentStation.Logo != null)
                            NowPlayingImage = GlobalHanasuCore.CurrentStation.Logo;
                        else
                            NowPlayingImage = DefaultAlbumArtUri;

                    if (CurrentArtistFromPlayer == null || CurrentSongFromPlayer == null) return;

                    Hanasu.Services.Notifications.NotificationsService.AddNotification(
                        LocalizationManager.GetLocalizedValue("NowPlayingGridHeader"),
                        CurrentArtistFromPlayer + " - " + CurrentSongFromPlayer, 3000, false,
                        Services.Notifications.NotificationType.Now_Playing, null, NowPlayingImage);
                }));

                x.Dispose();
            });
        }

        private Task CheckIfSongWasCaughtAtTheBeginning()
        {
            if (StationType == PlayerDetectedStationType.Shoutcast)
            {
                return Task.Factory.StartNew(() => Hanasu.Core.Stations.Shoutcast.ShoutcastService.GetShoutcastStationCurrentSongStartTime(GlobalHanasuCore.CurrentStation, DirectStationUrl.ToString()))
                    .ContinueWith(t =>
                    {
                        if (t.Exception == null)
                        {
                            var starttime = t.Result;

                            var diff = DateTime.Now.Subtract(starttime);

                            if (diff.TotalSeconds <= 25.0)
                            {
                                CurrentSongWasCaughtAtBeginning = true;
                            }
                            else
                            {
                                CurrentSongWasCaughtAtBeginning = false;

                            }
                        }

                        t.Dispose();
                    });
            }

            return null;
        }

        [MessageHandler("DisplayStationLogo")]
        public void HandleDisplayStationLogo(object data)
        {
            NowPlayingImage = (Uri)data;
        }

        public string StationTitleFromPlayer
        {
            get { return (dynamic)this.GetProperty("StationTitleFromPlayer"); }
            set { this.SetProperty("StationTitleFromPlayer", value); }
        }
        public string SongTitleFromPlayer
        {
            get { return (dynamic)this.GetProperty("SongTitleFromPlayer"); }
            set { this.SetProperty("SongTitleFromPlayer", value); }
        }

        public bool HasArtist
        {
            get { return (dynamic)this.GetProperty("HasArtist"); }
            set { this.SetProperty("HasArtist", value); }
        }

        public string CurrentArtistFromPlayer
        {
            get { return (dynamic)this.GetProperty("CurrentArtistFromPlayer"); }
            set
            {
                this.SetProperty("CurrentArtistFromPlayer", value);

                HasArtist = value != null;
            }
        }

        public string CurrentSongFromPlayer
        {
            get { return (dynamic)this.GetProperty("CurrentSongFromPlayer"); }
            set { this.SetProperty("CurrentSongFromPlayer", value); }
        }

        public dynamic CurrentArtistInfo
        {
            get { return (dynamic)this.GetProperty("CurrentArtistInfo"); }
            set { this.SetProperty("CurrentArtistInfo", value); }
        }

        public bool CurrentArtistInfoPaneIsOpen
        {
            get { return (bool)this.GetProperty("CurrentArtistInfoPaneIsOpen"); }
            set { this.SetProperty("CurrentArtistInfoPaneIsOpen", value); }
        }

        public bool LyricsPaneIsOpen
        {
            get { return (bool)this.GetProperty("LyricsPaneIsOpen"); }
            set { this.SetProperty("LyricsPaneIsOpen", value); }
        }

        public readonly static Uri DefaultAlbumArtUri = new Uri("http://www.ufomafia.com/radiographic.jpg");

        public Uri NowPlayingImage
        {
            get { return (Uri)this.GetProperty("NowPlayingImage"); }
            set { this.SetProperty("NowPlayingImage", value); }
        }
    }
}
