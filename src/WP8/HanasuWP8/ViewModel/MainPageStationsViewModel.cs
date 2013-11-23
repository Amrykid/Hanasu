using Crystal.Command;
using Crystal.Core;
using Crystal.Messaging;
using Crystal.Services;
using Hanasu.Core.Preprocessor;
using Hanasu.Model;
using IpcWrapper;
using Microsoft.Devices;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HanasuWP8.ViewModel
{
    public class MainPageStationsViewModel : BaseViewModel
    {
        private NamedEvent connectedEvent = null;
        private NamedEvent errorEvent = null;

        public MainPageStationsViewModel()
        {
            if (!IsDesignMode)
            {
                Initialize();

                PlayStationCommand = CommandManager.CreateCommand(async (y) =>
                    {
                        await PlayStation((Station)y);
                        return;
                    });
            }

            AreStationsChoosable = true;
        }

        private async Task PlayStation(Station y)
        {
            //if (IsBusy) return;

            if (!playCommandLock.WaitOne(5000)) return;

            Microsoft.Xna.Framework.FrameworkDispatcher.Update();
            if (!Microsoft.Xna.Framework.Media.MediaPlayer.GameHasControl)
                if (ServiceManager.Resolve<IMessageBoxService>()
                        .ShowOkayCancelMessage("Playing Media", "By proceeding, your current media will stop and the selected station will be played instead.") == false)
                {
                    playCommandLock.Set();
                    return;
                }


            //try
            //{
            //    if (App.IsPlaying)
            //    {
            //        IsBusy = true;

            //        Status = "Working...";

            //        try
            //        {
            //            //BackgroundAudioPlayer.Instance.Stop();
            //            //BackgroundAudioPlayer.Instance.Track = null;
            //            BackgroundAudioPlayer.Instance.Close();
            //            //BAPClosed = true;
            //        }
            //        catch (Exception) { }

            //        //await Task.Delay(2000);
            //    }
            //}
            //catch (Exception) { }

            Status = "Connecting...";

            IsBusy = true;
            AreStationsChoosable = false;

            Station station = (Station)y;

            var url = station.StreamUrl.Trim();

            if (BackgroundAudioPlayer.Instance.Volume != 0.5)
                BackgroundAudioPlayer.Instance.Volume = 0.5;

            bool preprocessingNeeded = false;
            try
            {
                if (preprocessingNeeded = await PreprocessorService.CheckIfPreprocessingIsNeeded(url))
                    url = (await PreprocessorService.GetProcessor(new Uri(url)).Process(new Uri(url))).ToString().Replace("\r/", "");
            }
            catch (Exception)
            {
                url = null;

                ServiceManager.Resolve<IMessageBoxService>().ShowMessage("Uh-oh!",
                        "Unable to retrieve this station's streaming url.");
            }

            if (url != null)
            {
                BackgroundAudioPlayer.Instance.Track = new AudioTrack(station.ServerType.ToLower() == "raw" ? new Uri(url) : null, station.Title, null, null, new Uri(station.ImageUrl),
                    "Hanasu$" + string.Join("$", station.Title, station.ServerType, url), EnabledPlayerControls.Pause);

                if (BAPClosed)
                {
                    BackgroundAudioPlayer.Instance.Play();
                    BAPClosed = false;
                }

                if (connectedEvent == null)
                {
                    await OpenEvents();
                }

                var playTask = connectedEvent.WaitAsync().AsTask();
                var errorTask = errorEvent.WaitAsync().AsTask();

                var timeoutTask = Task.Delay(System.Diagnostics.Debugger.IsAttached ? 12000 : 7000);

                var what = await Task.WhenAny(playTask, timeoutTask, errorTask);

                if ((what == timeoutTask || what == errorTask) && !App.IsPlaying)
                {
                    ServiceManager.Resolve<IMessageBoxService>().ShowMessage("Uh-oh!",
                        what == timeoutTask ? "Unable to connect in a timely fashion!" : what == errorTask ? "An error occurred while connecting." : "Something bad happened!");

#if DEBUG
                    var error = BackgroundAudioPlayer.Instance.Error;
                    if (System.Diagnostics.Debugger.IsAttached && error != null)
                    {
                        System.Diagnostics.Debugger.Log(4, "Exception", error.ToString());
                        System.Diagnostics.Debugger.Break();
                    }
#endif

                    BackgroundAudioPlayer.Instance.Stop();
                    //BackgroundAudioPlayer.Instance.Close();

                    //Status = "Rewinding state...";
                    //await Task.Delay(5000);

                    //BAPClosed = true;
                }
                else
                {
                    //Station is playing... or it should be.

                    Messenger.PushMessage(this, "SwitchTab", 0);
                }
            }

            Status = null;

            IsBusy = false;

            AreStationsChoosable = true;

            playCommandLock.Set();
        }

        private async Task OpenEvents()
        {
            while (true)
            {
                //loop until we get the event.
                if (NamedEvent.TryOpen(IPCConsts.CONNECTED_EVENT_NAME, out connectedEvent))
                {
                    while (true)
                    {
                        if (NamedEvent.TryOpen(IPCConsts.ERROR_EVENT_NAME, out errorEvent))
                        {
                            break;
                        }
                        await Task.Delay(100);
                    }
                    break;
                }

                await Task.Delay(100);
            }
        }

        private bool BAPClosed = false;
        private AutoResetEvent playCommandLock = new AutoResetEvent(true); //true means a station can be played.

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
        }

        public ObservableCollection<Station> Stations
        {
            get { return App.AvailableStations; }
        }

        public CrystalCommand PlayStationCommand
        {
            get { return GetPropertyOrDefaultType<CrystalCommand>(x => this.PlayStationCommand); }
            set { SetProperty(x => this.PlayStationCommand, value); }
        }


        public string Status
        {
            get { return GetPropertyOrDefaultType<string>(x => this.Status); }
            set
            {
                SetProperty(x => this.Status, value);
                Messenger.PushMessage(this, "UpdateIndeterminateStatus", new Tuple<bool, string>(IsBusy, value));
            }
        }
        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set
            {
                SetProperty(x => this.IsBusy, value);
                Messenger.PushMessage(this, "UpdateIndeterminateStatus", new Tuple<bool, string>(value, Status));
            }
        }

        public bool AreStationsChoosable
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.AreStationsChoosable); }
            set
            {
                SetProperty(x => this.AreStationsChoosable, value);
            }
        }
    }
}
