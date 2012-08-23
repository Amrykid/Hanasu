using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Hanasu.Core;
using Crystal.Command;
using System.Collections.ObjectModel;
using Hanasu.Core.Stations;
using System.Windows.Controls;
using Hanasu.UI;
using System.Windows.Data;
using Crystal.Localization;
using System.Windows;
using System.IO;
using Hanasu.Core.Preprocessor;
using Hanasu.View;
using Crystal.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Hanasu.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string AppDir = null;
        public MainWindowViewModel()
        {
            AppDir = new FileInfo(Application.ResourceAssembly.Location).DirectoryName;

            try
            {
                LocalizationManager.ProbeDirectory(AppDir + "\\I18N");
            }
            catch (Exception) { }

            GlobalHanasuCore.Initialize(new Func<string, object, object>(HandleEvents),
                AppDir + "\\Plugins\\");

            //LocalizationManager.ProbeDirectory

            CurrentVolume = 50;

            GlobalHanasuCore.SetVolume(CurrentVolume);

            SelectedStation = new Station();

            UIPanelState = FadeablePanelState.UpperFocus;

            IsPlaying = false;

            SwitchViewCommand = new CrystalCommand(this,
                true,
               (o) => UIPanelState = UIPanelState == FadeablePanelState.UpperFocus ? FadeablePanelState.LowerFocus : FadeablePanelState.UpperFocus);

            PlaySelectedStationCommand = this.CommandManager.CreateCommandFromBinding("SelectedStation",
                (s, e) =>
                    SelectedStation != null,
                new Action<object>(PlaySelectedStation));

            MediaPlayCommand = this.CommandManager.CreateCommandFromPropertyChangedAll(
                (s, e) =>
                    !IsPlaying && SelectedStation != null,
                    (o) => PlaySelectedStation(SelectedStation));

            MediaStopCommand = this.CommandManager.CreateCommandFromBinding("IsPlaying",
                (s, e) =>
                    IsPlaying,
                    new Action<object>(StopSelectedStation));

            VolumeHighButtonCommand = new CrystalCommand(this, true, (o) =>
                {
                    CurrentVolume = 80;
                });

            VolumeLowButtonCommand = new CrystalCommand(this, true, (o) =>
                {
                    CurrentVolume = 25;
                });

            VolumeMidButtonCommand = new CrystalCommand(this, true, (o) =>
                {
                    CurrentVolume = 50;
                });

            MediaFastForwardCommand = new NullCommand();
            MediaRewindCommand = new NullCommand();
        }

        /// <summary>
        /// Not sure if this should be in this view model or in the view's code behind
        /// </summary>
        public object UIBackPanelView
        {
            get { return this.GetProperty("UIBackPanelView"); }
            set { this.SetProperty("UIBackPanelView", value); }
        }


        [MessageHandler("WindowCommandLanguagesRequested")]
        public void ShowLanguageSelectionWindow(object data)
        {
            LanguageChooseWindow lcw = new LanguageChooseWindow();
            lcw.Owner = Application.Current.MainWindow;
            lcw.ShowDialog();
            lcw.Close();
        }


        public CrystalCommand VolumeLowButtonCommand { get; set; }
        public CrystalCommand VolumeMidButtonCommand { get; set; }
        public CrystalCommand VolumeHighButtonCommand { get; set; }

        public int CurrentVolume
        {
            get { return (int)this.GetProperty("CurrentVolume"); }
            set
            {
                this.SetProperty("CurrentVolume", value);

                GlobalHanasuCore.SetVolume(value);
            }
        }

        public CrystalCommand MediaRewindCommand { get; set; }
        public CrystalCommand MediaPlayCommand { get; set; }
        public CrystalCommand MediaStopCommand { get; set; }
        public CrystalCommand MediaFastForwardCommand { get; set; }
        public CrystalCommand PlaySelectedStationCommand { get; set; }

        public Station SelectedStation
        {
            get { return (Station)this.GetProperty("SelectedStation"); }
            set { this.SetProperty("SelectedStation", value); }
        }

        public ObservableCollection<Station> CatalogStations
        {
            get { return (ObservableCollection<Station>)this.GetProperty("CatalogStations"); }
            set { this.SetProperty("CatalogStations", value); }
        }

        public bool IsPlaying
        {
            get { return (bool)this.GetProperty("IsPlaying"); }
            set { this.SetProperty("IsPlaying", value); }
        }

        public CrystalCommand SwitchViewCommand { get; set; }
        public FadeablePanelState UIPanelState
        {
            get { return (FadeablePanelState)this.GetProperty("UIPanelState"); }
            set { this.SetProperty("UIPanelState", value); }
        }

        private object HandleEvents(string eventStr, object data)
        {
            switch (eventStr)
            {
                case GlobalHanasuCore.StationsUpdated:
                    {
                        CatalogStations = (dynamic)data;
                        break;
                    }
                case GlobalHanasuCore.StationTitleUpdated:
                    {
                        StationTitleFromPlayer = (string)data;
                        break;
                    }
                case GlobalHanasuCore.SongTitleUpdated:
                    {
                        SongTitleFromPlayer = (string)data;
                        break;
                    }
                case GlobalHanasuCore.NowPlayingStatus:
                    {
                        IsPlaying = (bool)data;
                        break;
                    }
                case GlobalHanasuCore.NowPlayingReset:
                    {
                        StationTitleFromPlayer = null;
                        SongTitleFromPlayer = null;

                        if (GlobalHanasuCore.CurrentStation.StationType == StationType.Radio)
                            UIBackPanelView = null;
                        break;
                    }
                case GlobalHanasuCore.StationConnectionError:
                    {
                        //TODO: Show some sort of error dialog.
                        break;
                    }
                case GlobalHanasuCore.MediaTypeDetected:
                    {
                        bool isvideo = (bool)data;

                        if (isvideo)
                        {
                            //UIPanelState = FadeablePanelState.LowerFocus;
                            UIBackPanelView = GlobalHanasuCore.GetPlayerView();
                        }

                        break;
                    }
                case GlobalHanasuCore.StationMultipleServersFound:
                    {
                        //Deal with choosing multiple stations.

                        IMultiStreamEntry[] entries = (dynamic)data;

                        Tuple<bool, IMultiStreamEntry> res = null;

                        ChooseStationStreamWindow cssw = new ChooseStationStreamWindow();

                        Messenger.PushMessage(this, "StationStreamWindowStreamsPushed", entries);

                        cssw.Owner = Application.Current.MainWindow;

                        Task.Factory.StartNew(() =>
                        {
                            res = new Tuple<bool, IMultiStreamEntry>(true, (IMultiStreamEntry)Messenger.WaitForMessage("StationStreamChoosen").Data);
                        }).ContinueWith(t => t.Dispose());

                        var dResult = cssw.ShowDialog();

                        if (dResult != true)
                            res = new Tuple<bool, IMultiStreamEntry>(false, null);
                        else
                        {
                            //Thread.Sleep(50); //May add this back if problems arise.
                            cssw.Close();
                        }

                        return res;
                    }
            }

            return null;
        }
        private void PlaySelectedStation(object o)
        {

            if (o == null) return;

            var stat = (Station)o;

            if (stat.Name == null) return;

            GlobalHanasuCore.StopStation();

            GlobalHanasuCore.PlayStation(stat);

            GlobalHanasuCore.SetVolume(CurrentVolume);
        }

        private void StopSelectedStation(object o)
        {
            GlobalHanasuCore.StopStation();
        }

        public string StationTitleFromPlayer
        {
            get { return (dynamic)this.GetProperty("StationTitleFromPlayer"); }
            set { this.SetProperty("StationTitleFromPlayer", value); }
        }
        public string SongTitleFromPlayer
        {
            get { return (dynamic)this.GetProperty("SongTitleFromPlayer"); }
            set { this.SetProperty("SongTitleFromPlayer", value); }
        }

        public object SelectedStationSource
        {
            get { return this.GetProperty("SelectedStationSource"); }
            set { this.SetProperty("SelectedStationSource", value); }
        }

        public string StationSearchFilter
        {
            get { return (string)this.GetProperty("StationSearchFilter"); }
            set
            {
                this.SetProperty("StationSearchFilter", value);
                HandleStationFilter();
            }
        }

        private void HandleStationFilter()
        {
            var x = CollectionViewSource.GetDefaultView(CatalogStations);

            if (string.IsNullOrWhiteSpace(StationSearchFilter))
                x.Filter = null;
            else
                x.Filter = new Predicate<object>(t =>
                    {
                        if (StationSearchFilter == null) return true;

                        var s = (Station)t;
                        return s.Name.ToLower().StartsWith(StationSearchFilter.ToLower())
                            || s.Name.ToLower().Contains(StationSearchFilter.ToLower());
                    });

            return;
        }
    }
}
