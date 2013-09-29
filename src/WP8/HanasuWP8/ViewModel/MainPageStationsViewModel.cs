using Crystal.Command;
using Crystal.Core;
using Crystal.Messaging;
using Hanasu.Core.Preprocessor;
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
    public class MainPageStationsViewModel : BaseViewModel
    {
        public MainPageStationsViewModel()
        {
            Initialize();

            PlayStationCommand = CommandManager.CreateCommand(async (y) =>
                {
                    if (IsBusy) return;

                    IsBusy = true;

                    Station x = (Station)y;

                    var url = x.StreamUrl;

                    if (await PreprocessorService.CheckIfPreprocessingIsNeeded(url))
                        url = (await PreprocessorService.GetProcessor(new Uri(url)).Process(new Uri(url))).ToString();

                    BackgroundAudioPlayer.Instance.Track = new AudioTrack(null, x.Title, null, null, new Uri(x.ImageUrl),
                        "Hanasu$" + string.Join("$", x.Title, x.ServerType, url), EnabledPlayerControls.Pause);

                    IsBusy = false;
                });
        }

        private async void Initialize()
        {
            IsBusy = true;
            await ((App)App.Current).LoadStationsTask;
            RaisePropertyChanged(x => this.Stations);
            IsBusy = false;
        }

        public ObservableCollection<Station> Stations
        {
            get { return ((App)App.Current).AvailableStations; }
        }

        public CrystalCommand PlayStationCommand
        {
            get { return GetPropertyOrDefaultType<CrystalCommand>(x => this.PlayStationCommand); }
            set { SetProperty(x => this.PlayStationCommand, value); }
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty(x => this.IsBusy, value); Messenger.PushMessage(this, "UpdateIndeterminateStatus", value); }
        }
    }
}
