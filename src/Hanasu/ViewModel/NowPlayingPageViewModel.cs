using Crystal.Core;
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
                if (!string.IsNullOrWhiteSpace(directUrl.Value))
                    await RefreshCurrentSongAndHistory(direct);
        }

        private KeyValuePair<string, string> directUrl;

        async void dt_Tick(object sender, object e)
        {
            if (Windows.UI.Xaml.Window.Current.Visible) //only fetch new info if the window is visible.. or else, its a waste of power.
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                    await RefreshCurrentSongAndHistory(directUrl));
        }

        private async Task RefreshCurrentSongAndHistory(KeyValuePair<string, string> direct)
        {
            if (NetworkCostController.CurrentNetworkingBehavior == NetworkingBehavior.Normal)
            {
                SongHistory = await ShoutcastService.GetShoutcastStationSongHistory(CurrentStation, direct.Value);

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
        }

        public Station CurrentStation { get; set; }

        public string Title { get; set; }

        public ImageSource Image { get; set; }

        public ObservableCollection<ShoutcastSongHistoryItem> SongHistory { get; set; }

        public string CurrentSong { get; set; }
    }
}
