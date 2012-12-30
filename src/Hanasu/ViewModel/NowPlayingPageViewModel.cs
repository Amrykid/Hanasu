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
using Windows.UI.Xaml.Media;

namespace Hanasu.ViewModel
{
    public class NowPlayingPageViewModel: Crystal.Dynamic.AutoIPNotifyingBaseViewModel
    {
        public NowPlayingPageViewModel()
        {
            SongHistory = new ObservableCollection<ShoutcastSongHistoryItem>();
        }
        public override void OnNavigatedFrom()
        {
            
        }
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


            await RefreshCurrentSongAndHistory(direct);
        }

        private async Task RefreshCurrentSongAndHistory(KeyValuePair<string, string> direct)
        {
            if (NetworkCostController.CurrentNetworkingBehavior == NetworkingBehavior.Normal)
            {
                SongHistory = await ShoutcastService.GetShoutcastStationSongHistory(CurrentStation, direct.Value);

                CurrentSong = SongHistory[0].Song;

                RaisePropertyChanged(x => this.CurrentSong);

                RaisePropertyChanged(x => this.SongHistory);
            }
        }

        public Station CurrentStation { get; set; }

        public string Title { get; set; }

        public ImageSource Image { get; set; }

        public ObservableCollection<ShoutcastSongHistoryItem> SongHistory { get; set; }

        public string CurrentSong { get; set; }
    }
}
