﻿using Crystal.Command;
using Crystal.Core;
using Crystal.Navigation;
using Hanasu.Model;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
                    BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;

                    if (App.Current != null)
                        ((HanasuWP8.App)App.Current).Exit2 += MainPageNowPlayingViewModel_Exit2;

                    if (timer == null)
                        timer = new DispatcherTimer();

                    timer.Tick += timer_Tick;
                    timer.Interval = new TimeSpan(0, 0, 3);

                    SongHistoryCommand = CommandManager.CreateCommand(x =>
                        {
                            NavigationService.NavigateTo<SongHistoryViewModel>(new KeyValuePair<string, string>("Station", CurrentStation.Title));
                        }, y => CurrentStation != null && CurrentStation.ServerType.ToLower() == "shoutcast");

                    SynchronizeBAPStatus();
                }
                else
                {
                    //False data for the design view.
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
            if (e != null && e is Microsoft.Phone.BackgroundAudio.PlayStateChangedEventArgs)
            {
                Microsoft.Phone.BackgroundAudio.PlayStateChangedEventArgs e2 = (Microsoft.Phone.BackgroundAudio.PlayStateChangedEventArgs)e;
                SynchronizeBAPStatus(e2);
            }
        }

        private async void SynchronizeBAPStatus(Microsoft.Phone.BackgroundAudio.PlayStateChangedEventArgs e = null)
        {
            DetectPlayState(e);

            try
            {
                if (BackgroundAudioPlayer.Instance.Track != null)
                {
                    if (BackgroundAudioPlayer.Instance.Track.Tag.StartsWith("Hanasu$"))
                    {
                        var data = BackgroundAudioPlayer.Instance.Track.Tag.ToString().Split('$');
                        var url = data[data.Length - 1];

                        await ((HanasuWP8.App)App.Current).LoadStationsTask;

                        Station currentStation = App.AvailableStations.First(x => x.Title == data[1]) as Station;

                        CurrentStation = currentStation;

                        UpdateNowPlaying();
                    }
                }
            }
            catch (Exception)
            {
            }

            if (IsPlaying)
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
            else

                timer.Stop();
        }

        private void DetectPlayState(Microsoft.Phone.BackgroundAudio.PlayStateChangedEventArgs e = null)
        {
            if (e == null)
                IsPlaying = BackgroundAudioPlayerExtensions.SafeGetPlayerState(BackgroundAudioPlayer.Instance) == PlayState.Playing; //try and guess
            else
                IsPlaying = e.CurrentPlayState == PlayState.Playing; //&& e.IntermediatePlayState == PlayState.BufferingStopped;
        }

        private void UpdateNowPlaying()
        {
            CurrentCover = CurrentStation.ImageUrl;

            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                try
                {
                    CurrentTrack = BackgroundAudioPlayer.Instance.Track.Title;
                    CurrentArtist = BackgroundAudioPlayer.Instance.Track.Artist;
                }
                catch (Exception) { }
            }

        }

        public override void OnSuspending(IDictionary<string, object> state, bool isRunningInBackground)
        {
            if (timer != null)
                timer.Stop();

            base.OnSuspending(state, isRunningInBackground);
        }
        public override void OnResuming(IDictionary<string, object> state)
        {
            if (timer != null)
                timer.Start();

            base.OnResuming(state);
        }

        public bool IsPlaying
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsPlaying); }
            set { SetProperty(x => this.IsPlaying, value); App.IsPlaying = value; }
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
            set { SetProperty(x => this.CurrentStation, value); SongHistoryCommand.RaiseCanExecuteChanged(); }
        }

        public CrystalCommand SongHistoryCommand
        {
            get { return GetPropertyOrDefaultType<CrystalCommand>(x => SongHistoryCommand); }
            set { SetProperty(x => SongHistoryCommand, value); }
        }
    }
}
