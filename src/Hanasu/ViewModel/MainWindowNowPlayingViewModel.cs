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

namespace Hanasu.ViewModel
{
    public class MainWindowNowPlayingViewModel : BaseViewModel
    {
        public MainWindowNowPlayingViewModel()
        {
            FindArtistInfoCommand = CommandManager.CreateCommandFromPropertyChangedAll((s, e) => CurrentArtistFromPlayer != null && IsFetchingArtistInfo == false, (o) =>
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

                                    Dispatcher.Invoke(new EmptyDelegate(() =>
                                        {
                                            CurrentArtistInfoPaneIsOpen = true;
                                        }));
                                }

                                IsFetchingArtistInfo = false;

                                t.Dispose();
                            });
                });

            CurrentArtistInfoPaneIsOpen = false;

            HasArtist = false;

            IsFetchingArtistInfo = false;
        }

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

        [MessageHandler("NowPlayingReset")]
        public void HandleNowPlayingReset(object data)
        {
            StationTitleFromPlayer = null;
            SongTitleFromPlayer = null;
            CurrentArtistFromPlayer = null;
            CurrentSongFromPlayer = null;
            CurrentArtistInfo = null;

            CurrentArtistInfoPaneIsOpen = false;

            NowPlayingImage = null;
        }

        [MessageHandler("SongTitleUpdated")]
        public void HandleSongTitleUpdated(object data)
        {
            SongTitleFromPlayer = (string)data;

            if (GlobalHanasuCore.CurrentSong.Artist != GlobalHanasuCore.CurrentStation.Name)
            {
                CurrentArtistFromPlayer = GlobalHanasuCore.CurrentSong.Artist;
                CurrentSongFromPlayer = GlobalHanasuCore.CurrentSong.TrackTitle;

                Task.Factory.StartNew(() => GlobalHanasuCore.GetExtendedSongInfoFromCurrentSong()).ContinueWith(x =>
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
                    }));

                    x.Dispose();
                });
            }
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

        public readonly static Uri DefaultAlbumArtUri = new Uri("http://www.ufomafia.com/radiographic.jpg");

        public Uri NowPlayingImage
        {
            get { return (Uri)this.GetProperty("NowPlayingImage"); }
            set { this.SetProperty("NowPlayingImage", value); }
        }
    }
}
