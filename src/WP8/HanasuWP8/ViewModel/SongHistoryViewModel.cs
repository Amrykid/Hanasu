using Crystal.Core;
using Hanasu.Model;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanasuWP8.ViewModel
{
    public class SongHistoryViewModel: BaseViewModel
    {
        public SongHistoryViewModel()
        {
            IsBusy = false;
        }

        public override void OnNavigatedTo(KeyValuePair<string, string>[] argument = null)
        {
            Station station = App.AvailableStations.First(x => x.Title == argument.First(y => y.Key == "Station").Value);

            LoadSongHistory(station);

            base.OnNavigatedTo(argument);
        }

        private async void LoadSongHistory(Station station)
        {
            IsBusy = true;

            var url = BackgroundAudioPlayer.Instance.Track.Tag.Split('$').Last();
            var songData = await Hanasu.Tools.Shoutcast.ShoutcastService.GetShoutcastStationSongHistory(station, url);
            SongHistory = songData;

            IsBusy = false;
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set
            {
                SetProperty(x => this.IsBusy, value);
            }
        }

        public ObservableCollection<Hanasu.Tools.Shoutcast.ShoutcastSongHistoryItem> SongHistory
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Hanasu.Tools.Shoutcast.ShoutcastSongHistoryItem>>(x => this.SongHistory); }
            set
            {
                SetProperty(x => this.SongHistory, value);
            }
        }
    }
}
