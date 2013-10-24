using Crystal.Core;
using Hanasu.Model;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HanasuWP8.ViewModel
{
    public class MainPageNowPlayingViewModel : BaseViewModel
    {

        private DispatcherTimer timer = new DispatcherTimer();
        public MainPageNowPlayingViewModel()
        {
            try
            {
                if (!IsDesignMode)
                {
                    if (!System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Launch();

                    BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
                    ((HanasuWP8.App)App.Current).Exit2 += MainPageNowPlayingViewModel_Exit2;

                    timer.Tick += timer_Tick;
                    timer.Interval = new TimeSpan(0, 0, 3);

                    SynchronizeBAPStatus();
                }
                else
                {
                    //Fale data for the design view.
                    IsPlaying = true;
                    CurrentCover = "https://si0.twimg.com/profile_images/1104224483/logo_transbkgr.png";
                    CurrentTrack = "Super-moo!";
                    CurrentArtist = "Amrykid";
                    CurrentStation = new Station() { Title = "AmryFM" };
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>().ShowMessage("Exception", ex.ToString());
#endif
            }
        }

        void MainPageNowPlayingViewModel_Exit2(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
        {
            ((HanasuWP8.App)App.Current).Exit2 -= MainPageNowPlayingViewModel_Exit2;
            BackgroundAudioPlayer.Instance.PlayStateChanged -= Instance_PlayStateChanged;
        }

        public override void OnNavigatedFrom()
        {

        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateNowPlaying();
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            SynchronizeBAPStatus();
        }

        private async void SynchronizeBAPStatus()
        {
            IsPlaying = BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing;

            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                if (BackgroundAudioPlayer.Instance.Track.Tag.StartsWith("Hanasu$"))
                {
                    var data = BackgroundAudioPlayer.Instance.Track.Tag.ToString().Split('$');
                    var url = data[data.Length - 1];

                    await ((HanasuWP8.App)App.Current).LoadStationsTask;

                    Station currentStation = ((App)App.Current).AvailableStations.First(x => x.Title == data[1]) as Station;

                    CurrentStation = currentStation;

                    UpdateNowPlaying();
                }
            }

            if (IsPlaying)
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
            else
                timer.Stop();
        }

        private void UpdateNowPlaying()
        {
            CurrentCover = CurrentStation.ImageUrl;

            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                CurrentTrack = BackgroundAudioPlayer.Instance.Track.Title;
                CurrentArtist = BackgroundAudioPlayer.Instance.Track.Artist;
            }

        }

        public bool IsPlaying
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsPlaying); }
            set { SetProperty(x => this.IsPlaying, value); }
        }
        public object CurrentCover
        {
            get { return GetPropertyOrDefaultType<object>(x => this.CurrentCover); }
            set { SetProperty(x => this.CurrentCover, value); }
        }
        public string CurrentTrack
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentTrack); }
            set { SetProperty(x => this.CurrentTrack, value); }
        }
        public string CurrentArtist
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentArtist); }
            set { SetProperty(x => this.CurrentArtist, value); }
        }
        public Station CurrentStation
        {
            get { return GetPropertyOrDefaultType<Station>(x => this.CurrentStation); }
            set { SetProperty(x => this.CurrentStation, value); }
        }

    }
}
