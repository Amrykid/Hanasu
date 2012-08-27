using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Messaging;
using Hanasu.Core;
using System.Threading.Tasks;
using Crystal.Command;

namespace Hanasu.ViewModel
{
    public class MainWindowNowPlayingViewModel : BaseViewModel
    {
        public MainWindowNowPlayingViewModel()
        {
            FindArtistInfoCommand = CommandManager.CreateCommandFromBinding("CurrentArtistFromPlayer", (s, e) => CurrentArtistFromPlayer != null, (o) =>
                {
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
                                }

                                t.Dispose();
                            });
                });

            HasArtist = false;
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

            NowPlayingImage = null;
        }

        [MessageHandler("SongTitleUpdated")]
        public void HandleSongTitleUpdated(object data)
        {
            SongTitleFromPlayer = (string)data;

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


        public readonly static Uri DefaultAlbumArtUri = new Uri("http://www.ufomafia.com/radiographic.jpg");

        public Uri NowPlayingImage
        {
            get { return (Uri)this.GetProperty("NowPlayingImage"); }
            set { this.SetProperty("NowPlayingImage", value); }
        }
    }
}
