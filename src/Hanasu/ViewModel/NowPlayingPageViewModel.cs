using Crystal.Core;
using Crystal.Localization;
using Hanasu.Model;
using Hanasu.SystemControllers;
using Hanasu.Tools.Shoutcast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hanasu.ViewModel
{
    public class NowPlayingPageViewModel : Crystal.Dynamic.AutoIPNotifyingBaseViewModel
    {
        public NowPlayingPageViewModel()
        {
            App.Current.Suspending += Current_Suspending;
            App.Current.Resuming += Current_Resuming;
            SongHistory = new ObservableCollection<ShoutcastSongHistoryItem>();
        }

        void Current_Resuming(object sender, object e)
        {
            if (dt_wasrunning)
                dt.Start();
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            dt_wasrunning = dt.IsEnabled;

            if (dt_wasrunning)
                dt.Stop();
        }
        public override void OnNavigatedFrom()
        {
            try
            {
                dt.Stop();
                dt.Tick -= dt_Tick;
            }
            catch (Exception)
            {
            }

            App.Current.Suspending -= Current_Suspending;
            App.Current.Resuming -= Current_Resuming;
        }
        bool dt_wasrunning = false;
        DispatcherTimer dt = new DispatcherTimer();
        public override async void OnNavigatedTo(dynamic argument = null)
        {
            //grab any arguments pass to the page when it was navigated to.

            if (argument == null) return;

            var args = (KeyValuePair<string, string>)argument[0];
            var direct = (KeyValuePair<string, string>)argument[1];

            CurrentStation = ((App)App.Current).AvailableStations.First(x => x.Title == args.Value);

            Title = CurrentStation.Title;
            RaisePropertyChanged(x => this.Title);

            Image = CurrentStation.Image;
            RaisePropertyChanged(x => this.Image);

            directUrl = direct;

            if (CurrentStation.ServerType.ToLower() == "shoutcast")
            {
                if (!string.IsNullOrWhiteSpace(directUrl.Value))
                    try
                    {
                        SongHistoryOperationStatus = SongHistoryOperationStatusType.Running;
                        await RefreshCurrentSongAndHistory(direct);
                        if (SongHistory != null && SongHistory.Count > 0)
                            SongHistoryOperationStatus = SongHistoryOperationStatusType.DataReturned;
                        else
                            SongHistoryOperationStatus = SongHistoryOperationStatusType.Ready;
                    }
                    catch (Exception)
                    {
                        if (dt.IsEnabled)
                        {
                            dt.Stop();
                            dt.Tick -= dt_Tick;
                        }
                    }
            }
            else
            {
                SongHistoryOperationStatus = SongHistoryOperationStatusType.UnsupportedServer;
            }
        }

        private KeyValuePair<string, string> directUrl;

        async void dt_Tick(object sender, object e)
        {
            if (Windows.UI.Xaml.Window.Current.Visible) //only fetch new info if the window is visible.. or else, its a waste of power.
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                {
                    SongHistoryOperationStatus = SongHistoryOperationStatusType.Running;
                    await RefreshCurrentSongAndHistory(directUrl);

                    if (SongHistory != null && SongHistory.Count > 0)
                        SongHistoryOperationStatus = SongHistoryOperationStatusType.DataReturned;
                    else
                        SongHistoryOperationStatus = SongHistoryOperationStatusType.Ready;
                });
        }

        private async Task RefreshCurrentSongAndHistory(KeyValuePair<string, string> direct)
        {
            if (NetworkCostController.CurrentNetworkingBehavior == NetworkingBehavior.Normal)
            {
                SongHistory = await ShoutcastService.GetShoutcastStationSongHistory(CurrentStation, direct.Value);

                for (int i = 0; i < SongHistory.Count; i++)
                {
                    var x = SongHistory[i];

                    x.LocalizedTime = LocalizationManager.FormatDateTime(x.Time);

                    SongHistory[i] = x;
                }

                CurrentSong = SongHistory[0].Song;

                RaisePropertyChanged(x => this.CurrentSong);

                RaisePropertyChanged(x => this.SongHistory);

                MediaControl.TrackName = CurrentSong;


                if (!dt.IsEnabled)
                {
                    dt.Interval = new TimeSpan(0, 2, 0);

                    dt.Tick += dt_Tick;

                    dt.Start();
                }
            }
            else
            {
                SongHistoryOperationStatus = SongHistoryOperationStatusType.NetworkStatusProhibits;
            }
        }

        public Station CurrentStation { get; set; }

        public string Title { get; set; }

        public ImageSource Image { get; set; }

        public ObservableCollection<ShoutcastSongHistoryItem> SongHistory { get; set; }
        public SongHistoryOperationStatusType SongHistoryOperationStatus
        {
            get { return (SongHistoryOperationStatusType)GetProperty("SongHistoryOperationStatus"); }
            set
            {
                SetProperty("SongHistoryOperationStatus", value);

                switch (value)
                {
                    case SongHistoryOperationStatusType.DataReturned:
                        SongHistoryOperationStatusMessage = "";
                        break;
                    case SongHistoryOperationStatusType.Running:
                        SongHistoryOperationStatusMessage = LocalizationManager.GetLocalizedValue("SongHistoryFetchingStatusMsg");
                        break;
                    case SongHistoryOperationStatusType.NetworkStatusProhibits:
                        SongHistoryOperationStatusMessage = LocalizationManager.GetLocalizedValue("SongHistoryNetworkConstraintsErrorMsg");
                        break;
                    case SongHistoryOperationStatusType.UnsupportedServer:
                        SongHistoryOperationStatusMessage = LocalizationManager.GetLocalizedValue("SongHistoryServerNotSupportedErrorMsg");
                        break;
                    case SongHistoryOperationStatusType.Ready:
                        SongHistoryOperationStatusMessage = LocalizationManager.GetLocalizedValue("SongHistoryReadyStatusMsg");
                        break;
                }
            }
        }
        public string SongHistoryOperationStatusMessage
        {
            get { return (string)GetProperty("SongHistoryOperationStatusMessage"); }
            set { SetProperty("SongHistoryOperationStatusMessage", value); }
        }

        public enum SongHistoryOperationStatusType
        {
            Ready = 0,
            Running = 1,
            DataReturned = 2,
            NetworkStatusProhibits = 3,
            UnsupportedServer = 4
        }

        public string CurrentSong { get; set; }
    }


}
