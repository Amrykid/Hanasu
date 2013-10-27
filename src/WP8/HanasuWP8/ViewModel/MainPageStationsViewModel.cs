﻿using Crystal.Command;
using Crystal.Core;
using Crystal.Messaging;
using Crystal.Services;
using Hanasu.Core.Preprocessor;
using Hanasu.Model;
using IpcWrapper;
using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HanasuWP8.ViewModel
{
    public class MainPageStationsViewModel : BaseViewModel
    {
        const string CONNECTED_EVENT_NAME = "HANASU_STREAM_CONNECTED";
        private NamedEvent connectedEvent = null;

        public MainPageStationsViewModel()
        {
            if (!IsDesignMode)
            {
                Initialize();

                PlayStationCommand = CommandManager.CreateCommand(async (y) =>
                    {
                        //if (IsBusy) return;

                        if (!playCommandLock.WaitOne(5000)) return;

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

                        Station x = (Station)y;

                        var url = x.StreamUrl.Trim();

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
                            BackgroundAudioPlayer.Instance.Track = new AudioTrack(x.ServerType.ToLower() == "raw" ? new Uri(url) : null, x.Title, null, null, new Uri(x.ImageUrl),
                                "Hanasu$" + string.Join("$", x.Title, x.ServerType, url), EnabledPlayerControls.Pause);

                            if (BAPClosed)
                            {
                                BackgroundAudioPlayer.Instance.Play();
                                BAPClosed = false;
                            }

                            while (true)
                            {
                                //loop until we get the event.
                                if (NamedEvent.TryOpen(CONNECTED_EVENT_NAME, out connectedEvent))
                                {
                                    break;
                                }

                                await Task.Delay(100);
                            }

                            var playTask = connectedEvent.WaitAsync().AsTask();

                            var timeoutTask = Task.Delay(System.Diagnostics.Debugger.IsAttached ? 12000 : 7000);

                            var what = await Task.WhenAny(playTask, timeoutTask);

                            if (what == timeoutTask && !App.IsPlaying)
                            {
                                ServiceManager.Resolve<IMessageBoxService>().ShowMessage("Uh-oh!",
                                    "Unable to connect in a timely fashion!");

#if DEBUG
                                var error = BackgroundAudioPlayer.Instance.Error;
                                if (System.Diagnostics.Debugger.IsAttached && error != null)
                                {
                                    System.Diagnostics.Debugger.Log(4, "Exception", error.ToString());
                                    System.Diagnostics.Debugger.Break();
                                }
#endif

                                BackgroundAudioPlayer.Instance.Stop();
                                BackgroundAudioPlayer.Instance.Close();

                                Status = "Rewinding state...";
                                await Task.Delay(5000);

                                BAPClosed = true;
                            }
                            else
                            {
                                Messenger.PushMessage(this, "SwitchTab", 0);
                                //.Instance.Play();
                            }
                        }

                        Status = null;

                        IsBusy = false;

                        playCommandLock.Set();
                    });
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
    }
}
