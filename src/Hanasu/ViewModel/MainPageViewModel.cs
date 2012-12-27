using Crystal.Core;
using Crystal.Localization;
using Hanasu.Extensions;
using Hanasu.Model;
using Hanasu.SystemControllers;
using Hanasu.Tools.Shoutcast;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media;

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
                    if (mediaElement != null && CurrentStation != null)
                        if (mediaElement.CurrentState != MediaElementState.Playing)
                            PlayStation(CurrentStation, mediaElement);
                });

            PauseCommand = CommandManager.CreateCommand(() =>
                {
                    if (mediaElement != null && CurrentStation != null)
                        if (mediaElement.CurrentState != MediaElementState.Paused)
                            mediaElement.Pause();
                });
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

        }

        void MediaControl_PlayPressed(object sender, object e)
        {

        }

        void MediaControl_PausePressed(object sender, object e)
        {

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

            PlayToController.Initialize(ref mediaElement);
            NetworkCostController.ApproachingDataLimitEvent += NetworkCostController_ApproachingDataLimitEvent;
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

        async void mediaElement_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!Windows.UI.Xaml.Window.Current.Visible)
            {
                SendToast(
                    string.Format(
                        LocalizationManager.GetLocalizedValue("NowStreamingMsg"), 
                        CurrentStationName), CurrentStation.ImageUrl);
            }

            try
            {
                if (CurrentStation.ServerType.ToLower() == "shoutcast")
                {
                    var currentSong = await ShoutcastService.GetShoutcastStationCurrentSong(CurrentStation, CurrentStationStreamedUri.ToString());

                    CurrentStationSongData = currentSong.ToSongString();
                }
            }
            catch (Exception)
            {
            }
        }

        void mediaElement_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            CurrentStationSongData = null;
            CurrentStationStreamedUri = null;
        }

        void mediaElement_BufferingProgressChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        public ICommand PlayCommand { get; set; }
        public ICommand PauseCommand { get; set; }


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
                    }));
                    break;
                case false:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        mediaElement.PlaybackRate = 1;
                        MediaControl.IsPlaying = true;
                    }));
                    break;
            }
        }

        private void LoadStationsFromAppGlobal()
        {
            AvailableStations = new ObservableCollection<StationGroup>();

            var stations = ((App)App.Current).AvailableStations;

            if (stations == null)
            {
                ((App)App.Current).LoadStations();
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
                    sGroup.Items.Add(i);

                AvailableStations.Add(sGroup);
            }

            RaisePropertyChanged(x => this.AvailableStations);
        }

        public ObservableCollection<StationGroup> AvailableStations
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<StationGroup>>(x => this.AvailableStations); }
            set { SetProperty(x => this.AvailableStations, value); }
        }



        [Crystal.Messaging.MessageHandler("PlayStation")]
        public void PlayStation(Station s)
        {
            PlayStation(s, mediaElement);
        }
        public async void PlayStation(Station s, Windows.UI.Xaml.Controls.MediaElement me)
        {
            if (!NetworkCostController.IsConnectedToInternet)
            {
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("InternetConnectionHeader"),
                        LocalizationManager.GetLocalizedValue("NoInternetConnectionMsg"));
                return;
            }

            if (NetworkCostController.ApproachingDataLimit)
            {
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("DataConstraintsHeader"),
                        LocalizationManager.GetLocalizedValue("StreamingDisabled2Msg"));

                return;
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    SetMediaElement(ref me);

                    CurrentStation = s;
                    CurrentStationSongData = null;
                    CurrentStationStreamedUri = null;
                });

            //MediaControl.AlbumArt = new Uri(CurrentStation.ImageUrl, UriKind.Absolute);
            //MediaControl.ArtistName = CurrentStation.Title;
            MediaControl.IsPlaying = true;


            if (me.CurrentState == MediaElementState.Playing || me.CurrentState == MediaElementState.Opening || me.CurrentState == MediaElementState.Buffering)
            {
                me.Pause();
                await Task.Delay(1000);
            }

            Uri finalUri = new Uri(s.StreamUrl, UriKind.Absolute);

            if (await Hanasu.Core.Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(finalUri, s.PreprocessorFormat))
                finalUri = await Hanasu.Core.Preprocessor.PreprocessorService.GetProcessor(finalUri, s.PreprocessorFormat).Process(finalUri);

            CurrentStationStreamedUri = finalUri;

            await mediaElement.OpenAsync(finalUri, System.Threading.CancellationToken.None);
            await mediaElement.PlayAsync(System.Threading.CancellationToken.None);

        }

        public Uri CurrentStationStreamedUri
        {
            get { return GetPropertyOrDefaultType<Uri>(x => this.CurrentStationStreamedUri); }
            set { SetProperty(x => this.CurrentStationStreamedUri, value); }
        }

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


        private void SendToast(string txt, string imgurl = "")
        {
            var toaster = ToastNotificationManager.CreateToastNotifier();

            if (toaster.Setting != NotificationSetting.Enabled) return;

            // It is possible to start from an existing template and modify what is needed.
            // Alternatively you can construct the XML from scratch.
            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            var title = toastXml.CreateElement("toast");
            var visual = toastXml.CreateElement("visual");
            visual.SetAttribute("version", "1");
            visual.SetAttribute("lang", "en-US");

            // The template is set to be a ToastImageAndText01. This tells the toast notification manager what to expect next.
            var binding = toastXml.CreateElement("binding");
            binding.SetAttribute("template", "ToastImageAndText01");

            // An image element is then created under the ToastImageAndText01 XML node. The path to the image is specified
            var image = toastXml.CreateElement("image");
            image.SetAttribute("id", "1");
            image.SetAttribute("src", imgurl);

            // A text element is created under the ToastImageAndText01 XML node.
            var text = toastXml.CreateElement("text");
            text.SetAttribute("id", "1");
            text.InnerText = txt;

            // All the XML elements are chained up together.
            title.AppendChild(visual);
            visual.AppendChild(binding);
            binding.AppendChild(image);
            binding.AppendChild(text);

            toastXml.AppendChild(title);

            // Create a ToastNotification from our XML, and send it to the Toast Notification Manager
            var toast = new ToastNotification(toastXml);
            toaster.Show(toast);
        }
    }
}
