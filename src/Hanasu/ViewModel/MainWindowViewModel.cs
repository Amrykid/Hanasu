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

namespace Hanasu.ViewModel
{
    public class MainWindowViewModel: BaseViewModel
    {
        public string AppDir = null;
        public MainWindowViewModel()
        {
            AppDir = new FileInfo(Application.ResourceAssembly.Location).DirectoryName;

            GlobalHanasuCore.Initialize(new Action<string, object>(HandleEvents), 
                AppDir + "\\Plugins\\");

            //LocalizationManager.ProbeDirectory

            SelectedStation = new Station();

            UIPanelState = FadeablePanelState.UpperFocus;

            IsPlaying = false;

            SwitchViewCommand = new CrystalCommand(this,
                true,
               (o) => UIPanelState = UIPanelState == FadeablePanelState.UpperFocus ? FadeablePanelState.LowerFocus : FadeablePanelState.UpperFocus);

            PlaySelectedStationCommand = this.CreateCommandFromBinding("SelectedStation", 
                (s, e) => 
                    SelectedStation != null, 
                new Action<object>(PlaySelectedStation));

            InitializeViews();
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

        private void HandleEvents(string eventStr, object data)
        {
            switch (eventStr)
            {
                case GlobalHanasuCore.StationsUpdated:
                    {
                        CatalogStations = (dynamic)data;
                        break;
                    }
            }
        }

        private void PlaySelectedStation(object o)
        {
        }

        #region Related to view selection
        public ViewBase CurrentSelectedView
        {
            get { return (ViewBase)this.GetProperty("CurrentSelectedView");}
            set { this.SetProperty("CurrentSelectedView",value);}
        }

        public GridView GridViewObject { get; set; }
        public ImageHeaderView ImageViewObject { get; set; }

        private void InitializeViews()
        {
            GridViewObject = new GridView();
            GridViewObject.Columns.Add(new GridViewColumn() { Header = LocalizationManager.GetLocalizedValue("StationNameColumn"), DisplayMemberBinding = new Binding("Name") });
            GridViewObject.Columns.Add(new GridViewColumn() { Header = LocalizationManager.GetLocalizedValue("StationLanguageColumn"), DisplayMemberBinding = new Binding("Language") });

            ImageViewObject = new ImageHeaderView();

            CurrentSelectedView = GridViewObject;
        }
        #endregion
    }
}
