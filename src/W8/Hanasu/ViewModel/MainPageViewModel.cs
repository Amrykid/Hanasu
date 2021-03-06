﻿using Crystal.Core;
using Crystal.Localization;
using Hanasu.Extensions;
using Hanasu.Model;
using Hanasu.SystemControllers;
using Hanasu.Tools.Shoutcast;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using Crystal.Navigation;
using Crystal.Command;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Hanasu.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public MainPageViewModel()
        {
            LoadStationsFromAppGlobal();

            RegisterWithMediaTransportControls();

            PlayCommand = CommandManager.CreateCommand(() =>
                {
                    if (mediaElement != null)
                        if (mediaElement.CurrentState != MediaElementState.Playing)
                        {
                            PlayStation(CurrentStation, mediaElement, false);
                            IsPlaying = true;
                            SetMediaButtons();
                        }
                });

            PauseCommand = CommandManager.CreateCommand(() =>
                {
                    if (mediaElement != null)
                        if (mediaElement.CurrentState != MediaElementState.Paused)
                        {
                            mediaElement.Pause();
                            IsPlaying = false;
                            SetMediaButtons();
                        }
                });

            NextStationCommand = CommandManager.CreateCommand(() =>
                {
                });

            PreviousStationCommand = CommandManager.CreateCommand(() =>
                {
                });

            SetMediaButtons();
        }
        public override void OnNavigatedFrom()
        {
            if (mediaElement != null)
            {
                mediaElement.BufferingProgressChanged -= mediaElement_BufferingProgressChanged;
                mediaElement.MediaFailed -= mediaElement_MediaFailed;
                mediaElement.MediaOpened -= mediaElement_MediaOpened;
                mediaElement.MediaEnded -= mediaElement_MediaEnded;
            }

            ((App)App.Current).MediaElement.CurrentStateChanged -= mediaElement_CurrentStateChanged;

            //NetworkCostController.InternetConnectionChanged -= NetworkCostController_InternetConnectionChanged;
        }

        #region Registering these events are required for playing media in the background AND in order to play from a mediaelement that is set to BackgroundCompatibleMedia.
        //If they are not used, app will hang.
        private void RegisterWithMediaTransportControls()
        {
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.SoundLevelChanged += MediaControl_SoundLevelChanged;
            MediaControl.ChannelDownPressed += MediaControl_ChannelDownPressed;
            MediaControl.ChannelUpPressed += MediaControl_ChannelUpPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
        }

        ~MainPageViewModel()
        {
            MediaControl.PlayPauseTogglePressed -= MediaControl_PlayPauseTogglePressed;
            MediaControl.SoundLevelChanged -= MediaControl_SoundLevelChanged;
            MediaControl.ChannelDownPressed -= MediaControl_ChannelDownPressed;
            MediaControl.ChannelUpPressed -= MediaControl_ChannelUpPressed;
            MediaControl.PausePressed -= MediaControl_PausePressed;
            MediaControl.PlayPressed -= MediaControl_PlayPressed;
            MediaControl.StopPressed -= MediaControl_StopPressed;
        }

        void MediaControl_StopPressed(object sender, object e)
        {
            // hardware/keyboard stop key

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low,
                () =>
                {
                    try
                    {
                        if (mediaElement.CurrentState != MediaElementState.Stopped)
                            mediaElement.Stop();
                    }
                    catch (Exception)
                    {
                    }
                });
        }

        void MediaControl_PlayPressed(object sender, object e)
        {
            // hardware/keyboard play key
        }

        void MediaControl_PausePressed(object sender, object e)
        {
            // hardware/keyboard pause key
        }

        void MediaControl_ChannelUpPressed(object sender, object e)
        {

        }

        void MediaControl_ChannelDownPressed(object sender, object e)
        {

        }
        #endregion

        #region Badly written mediaelement stuff
        internal void SetMediaElement(ref Windows.UI.Xaml.Controls.MediaElement me)
        {
            if (mediaElement == null)
            {
                mediaElement = me;

                InitializeGlobalMediaElement();
            }
        }
        internal void SetMediaElement(Windows.UI.Xaml.Controls.MediaElement me)
        {
            if (mediaElement == null)
            {
                mediaElement = me;

                InitializeGlobalMediaElement();
            }
        }

        private Windows.UI.Xaml.Controls.MediaElement mediaElement = null; //what

        private void InitializeGlobalMediaElement()
        {
            mediaElement.AudioCategory = AudioCategory.BackgroundCapableMedia;
            mediaElement.AudioDeviceType = AudioDeviceType.Multimedia;
            mediaElement.AutoPlay = false;

            mediaElement.BufferingProgressChanged += mediaElement_BufferingProgressChanged;
            mediaElement.MediaFailed += mediaElement_MediaFailed;
            mediaElement.MediaOpened += mediaElement_MediaOpened;
            mediaElement.MediaEnded += mediaElement_MediaEnded;

            PlayToController.Initialize(mediaElement);
            NetworkCostController.ApproachingDataLimitEvent += NetworkCostController_ApproachingDataLimitEvent;
        }

        void mediaElement_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SetMediaButtons();
        }

        private void SetMediaButtons()
        {
            if (mediaElement == null)
            {
                PlayCommand.SetCanExecute(false);
                PauseCommand.SetCanExecute(false);
            }
            else
            {
                IsPlaying = mediaElement.CurrentState == MediaElementState.Playing || mediaElement.CurrentState == MediaElementState.Buffering || mediaElement.CurrentState == MediaElementState.Opening; //Detect status

                switch (IsPlaying) //MediaElement.CurrentState
                {
                    case true:
                        PlayCommand.SetCanExecute(false);
                        PauseCommand.SetCanExecute(true);
                        break;
                    case false:
                        PlayCommand.SetCanExecute(true);
                        PauseCommand.SetCanExecute(false);
                        break;
                }
            }
        }

        void mediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ResetMediaControlInfo();
        }

        void NetworkCostController_ApproachingDataLimitEvent()
        {
            Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                .ShowMessage(
                    LocalizationManager.GetLocalizedValue("DataConstraintsHeader"),
                    LocalizationManager.GetLocalizedValue("StreamingDisabledMsg"));

            mediaElement.Stop();
        }
        #endregion

        void mediaElement_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!Windows.UI.Xaml.Window.Current.Visible)
            {
                NotificationsController.SendToast(
                    string.Format(
                        LocalizationManager.GetLocalizedValue("NowStreamingMsg"),
                        CurrentStationName), CurrentStation.ImageUrl);
            }

            IsPlaying = true;
        }

        void mediaElement_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            CurrentStationSongData = null;
            CurrentStationStreamedUri = null;
            IsPlaying = false;

            ResetMediaControlInfo();
        }

        private static void ResetMediaControlInfo()
        {
            MediaControl.AlbumArt = null;
            MediaControl.ArtistName = string.Empty;
            MediaControl.TrackName = string.Empty;
        }

        void mediaElement_BufferingProgressChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        public bool IsPlaying
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsPlaying); }
            set { SetProperty<bool>(x => this.IsPlaying, value); }
        }

        public CrystalCommand PlayCommand { get; set; }
        public CrystalCommand PauseCommand { get; set; }
        public CrystalCommand PreviousStationCommand { get; set; }
        public CrystalCommand NextStationCommand { get; set; }


        async void MediaControl_SoundLevelChanged(object sender, object e)
        {
            switch (MediaControl.SoundLevel)
            {
                case SoundLevel.Full:
                case SoundLevel.Low:
                    await Dispatcher.RunIdleAsync(new Windows.UI.Core.IdleDispatchedHandler((x) =>
                    {
                        if (mediaElement == null) return;

                        if (mediaElement.CurrentState == MediaElementState.Stopped || mediaElement.CurrentState == MediaElementState.Paused)
                            mediaElement.Play();
                    }));
                    break;
                case SoundLevel.Muted:
                    await Dispatcher.RunIdleAsync(new Windows.UI.Core.IdleDispatchedHandler((x) =>
                    {
                        if (mediaElement == null) return;

                        mediaElement.Pause();
                    }));
                    break;
            }
        }

        async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            switch (MediaControl.IsPlaying) //the old value, not the new one
            {
                case true: //was playing, should pause
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        mediaElement.PlaybackRate = 0;
                        MediaControl.IsPlaying = false;
                        IsPlaying = false;
                    }));
                    break;
                case false:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        mediaElement.PlaybackRate = 1;
                        MediaControl.IsPlaying = true;
                        IsPlaying = true;
                    }));
                    break;
            }
        }

        private async void LoadStationsFromAppGlobal()
        {
            if (NetworkCostController.IsConnectedToInternet)
            {
                AvailableStations = new ObservableCollection<StationGroup>();

                var stations = ((App)App.Current).AvailableStations;

                if (stations == null)
                {
                    await ((App)App.Current).LoadStations();
                    stations = ((App)App.Current).AvailableStations;
                }

                var formats = stations.Select(x => x.UnlocalizedFormat).Distinct();

                foreach (var format in formats)
                {
                    var sGroup = new StationGroup();
                    sGroup.Name = LocalizationManager.GetLocalizedValue("Group" + format);
                    sGroup.UnlocalizedName = format;
                    sGroup.Items = new ObservableCollection<Station>();

                    foreach (var i in stations.Where(x => x.UnlocalizedFormat == format).Take(2))
                    {
                        i.StationDisplay = StationDisplayType.Main;
                        sGroup.Items.Add(i);
                    }

                    AvailableStations.Add(sGroup);
                }

                RaisePropertyChanged(x => this.AvailableStations);
            }
        }

        public ObservableCollection<StationGroup> AvailableStations
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<StationGroup>>(x => this.AvailableStations); }
            set { SetProperty(x => this.AvailableStations, value); }
        }



        [Crystal.Messaging.MessageHandler("PlayStation")]
        public void PlayStation(Station s)
        {
            if (mediaElement == null)
                mediaElement = ((App)App.Current).MediaElement;

            PlayStation(s, mediaElement);
        }
        public async void PlayStation(Station s, Windows.UI.Xaml.Controls.MediaElement me, bool navigate = true)
        {
            if (s == null) return;

            if (!NetworkCostController.IsConnectedToInternet) //makes sure Hanasu is connected to the internet.
            {
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("InternetConnectionHeader"),
                        LocalizationManager.GetLocalizedValue("NoInternetConnectionMsg"));
                return;
            }

            if (NetworkCostController.CurrentNetworkingBehavior == NetworkingBehavior.Opt_In) //if the user is roaming and/or over the data limit, notify them
            {
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("DataConstraintsHeader"),
                        LocalizationManager.GetLocalizedValue("StreamingDisabled2Msg"));

                return;
            }

            // Reset things things are ready to be played.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    SetMediaElement(ref me);

                    CurrentStation = s;
                    CurrentStationSongData = null;
                    CurrentStationStreamedUri = null;
                });

            StorageFile albumFile = await GetStationAlbumFromCache(); //Grab the current station's logo from the cache.

            if (albumFile != null)
                MediaControl.AlbumArt = new Uri("ms-appdata:///local/Hanasu/" + albumFile.DisplayName); //Set the logo in the media control.
            MediaControl.ArtistName = CurrentStation.Title; //set the station's name
            MediaControl.IsPlaying = true;

            try
            {

                if (me.CurrentState == MediaElementState.Playing || me.CurrentState == MediaElementState.Opening || me.CurrentState == MediaElementState.Buffering)
                {
                    me.Pause();
                    await Task.Delay(1000);
                    me.Stop();
                }

                Uri finalUri = new Uri(s.StreamUrl, UriKind.Absolute);

                if (await Hanasu.Core.Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(finalUri, s.PreprocessorFormat))
                    finalUri = await Hanasu.Core.Preprocessor.PreprocessorService.GetProcessor(finalUri, s.PreprocessorFormat).Process(finalUri);

                CurrentStationStreamedUri = finalUri;

                //if (CurrentStation.ServerType.ToLower() == "shoutcast")
                //{
                //    var str = await ShoutcastService.GetShoutcastStream(finalUri);
                //    mediaElement.SetSource(str, str.Content_Type);

                //    mediaElement.Play();
                //}
                //else
                //{

                //finalUri = new Uri(finalUri.ToString() + ";stream.nsv", UriKind.Absolute);

                try
                {
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

                    var openTask = mediaElement.OpenAsync(finalUri, cts.Token);
                    var timeoutTask = Task.Delay(10000); //Wait for a connection for 10 seconds.

                    var successful = await Task.WhenAny(openTask, timeoutTask);

                    if (successful == timeoutTask)
                    {
                        //timeout. inform the user and back out.

                        IsPlaying = false;

                        Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                            .ShowMessage(
                                 LocalizationManager.GetLocalizedValue("StreamingErrorHeader"),
                                 LocalizationManager.GetLocalizedValue("StreamingConnectionTimeoutMsg"));

                        cts.Cancel();
                        cts.Dispose();

                        ResetStationInfo();

                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException) return;

                    throw ex;
                }

                if (navigate && IsPlaying)
                    NavigateToNowPlayingPage(finalUri);

                try
                {
                    if (!PlayToController.IsConnectedViaPlayTo)
                    {
                        var playTask = mediaElement.PlayAsync(System.Threading.CancellationToken.None);
                        IsPlaying = true;
                        await playTask;
                    }
                    else
                        IsPlaying = true;
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException) return;

                    throw ex;
                }

                //}
            }
            catch (Exception ex)
            {
                if (mediaElement.CurrentState == MediaElementState.Playing || mediaElement.CurrentState == MediaElementState.Opening) return; //Ignorable error. Probably nothing.

                IsPlaying = false;

                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("StreamingErrorHeader"),
                        LocalizationManager.GetLocalizedValue("StreamingErrorMsg"));
            }

        }

        private void ResetStationInfo()
        {
            CurrentStation = null;
            CurrentStationSongData = null;
            CurrentStationStreamedUri = null;
        }

        private void NavigateToNowPlayingPage(Uri finalUri)
        {
            //if its made it this far, try navigating to the "now playing page".
            if (NavigationService.IsNavigatedTo<NowPlayingPageViewModel>()) return;

            NavigationService.NavigateTo<NowPlayingPageViewModel>(new KeyValuePair<string, object>("station", CurrentStationName), new KeyValuePair<string, object>("directurl", finalUri != null ? finalUri.ToString() : ""));
        }
        public void NavigateToNowPlayingPage()
        {
            NavigateToNowPlayingPage(CurrentStationStreamedUri);
        }

        /// <summary>
        /// Grabs the current station's album art from a cached file.
        /// </summary>
        /// <returns></returns>
        private async Task<StorageFile> GetStationAlbumFromCache()
        {
            StorageFile albumFile = null;

            try
            {
                albumFile = await App.AppFolder.CreateFileAsync(CurrentStation.Title + CurrentStation.ImageUrl.Substring(CurrentStation.ImageUrl.LastIndexOf(".")));

                var str = await albumFile.OpenAsync(FileAccessMode.ReadWrite);

                var http = new HttpClient();
                var data = await http.GetByteArrayAsync(CurrentStation.ImageUrl);

                await str.WriteAsync(data.AsBuffer());
                await str.FlushAsync();
                str.Dispose();
                http.Dispose();
            }
            catch (Exception)
            {

            }
            try
            {
                if (albumFile == null)
                    albumFile = await App.AppFolder.GetFileAsync(CurrentStation.Title + CurrentStation.ImageUrl.Substring(CurrentStation.ImageUrl.LastIndexOf(".")));
            }
            catch (Exception)
            {
            }
            return albumFile;
        }

        public Uri CurrentStationStreamedUri
        {
            get { return GetPropertyOrDefaultType<Uri>(x => this.CurrentStationStreamedUri); }
            set { SetProperty(x => this.CurrentStationStreamedUri, value); }
        }

        [Crystal.Messaging.PropertyMessage("CurrentStation")]
        public Station CurrentStation
        {
            get { return GetPropertyOrDefaultType<Station>(x => this.CurrentStation); }
            set { SetProperty(x => this.CurrentStation, value); RaisePropertyChanged(x => this.CurrentStationName); }
        }

        public string CurrentStationName
        {
            get { if (CurrentStation != null) return CurrentStation.Title; else return string.Empty; }
        }

        public string CurrentStationSongData
        {
            get { return GetPropertyOrDefaultType<string>(x => this.CurrentStationSongData); }
            set { SetProperty(x => this.CurrentStationSongData, value); }
        }

        public override async void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            await Task.Delay(1000);

            ((App)App.Current).MediaElement.CurrentStateChanged += mediaElement_CurrentStateChanged;

            SetMediaButtons();

            NetworkCostController.InternetConnectionChanged += NetworkCostController_InternetConnectionChanged;

            //grab any arguments pass to the mainpage when it was navigated to.

            if (argument == null) return;

            var args = (KeyValuePair<string, object>)argument[0];

            switch (args.Key.ToLower())
            {
                case "stationtoplay": //plays a station if it was passed from another page/viewmodel
                    {
                        var stat = ((App)App.Current).AvailableStations.First(x => x.Title == args.Value);

                        PlayStation(stat, ((App)App.Current).MediaElement);

                        break;
                    }
            }
        }

        async void NetworkCostController_InternetConnectionChanged()
        {
            if (NetworkCostController.IsConnectedToInternet)
            {
                if (AvailableStations == null || AvailableStations.Count == 0)
                {
                    if (((App)App.Current).AvailableStations == null || ((App)App.Current).AvailableStations.Count == 0)
                        await ((App)App.Current).LoadStations();

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            LoadStationsFromAppGlobal();
                        });

                }
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    AvailableStations.Clear();
                    RaisePropertyChanged(x => this.AvailableStations);
                });
            }
        }
    }
}
