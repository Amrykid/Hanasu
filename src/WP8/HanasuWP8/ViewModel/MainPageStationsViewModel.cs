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
            if (!IsDesignMode)
            {
                Initialize();

                PlayStationCommand = CommandManager.CreateCommand(async (y) =>
                    {
                        //if (IsBusy) return;
                        try
                        {
                            BackgroundAudioPlayer.Instance.Stop();
                            BackgroundAudioPlayer.Instance.Close();
                        }
                        catch (Exception) { }

                        Messenger.PushMessage(this, "SwitchTab", 0);

                        Status = "Connecting...";

                        IsBusy = true;

                        Station x = (Station)y;

                        var url = x.StreamUrl.Trim();

                        if (await PreprocessorService.CheckIfPreprocessingIsNeeded(url))
                            url = (await PreprocessorService.GetProcessor(new Uri(url)).Process(new Uri(url))).ToString().Replace("\r/","");

                        BackgroundAudioPlayer.Instance.Track = new AudioTrack(null, x.Title, null, null, new Uri(x.ImageUrl),
                            "Hanasu$" + string.Join("$", x.Title, x.ServerType, url), EnabledPlayerControls.Pause);
                    });
            }
        }

        private async void Initialize()
        {
            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
            ((HanasuWP8.App)App.Current).Exit2 += MainPageStationsViewModel_Exit2;

            IsBusy = true;
            await ((App)App.Current).LoadStationsTask;
            RaisePropertyChanged(x => this.Stations);
            IsBusy = false;
        }

        void MainPageStationsViewModel_Exit2(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
        {
            BackgroundAudioPlayer.Instance.PlayStateChanged -= Instance_PlayStateChanged;
            ((HanasuWP8.App)App.Current).Exit2 -= MainPageStationsViewModel_Exit2;
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                Status = null;

                IsBusy = false;
            }
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


        public string Status { get; set; }
        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set
            {
                SetProperty(x => this.IsBusy, value);
                Messenger.PushMessage(this, "UpdateIndeterminateStatus", new Tuple<bool, string>(value, Status));
            }
        }
    }
}
