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
using Crystal.Services;
using Hanasu.Services.Notifications;
using Hanasu.Misc.HTTPd;
using System.Xml.Linq;

namespace Hanasu.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string AppDir = null;
        public MainWindowViewModel()
        {
            if (IsDesignMode) return;

            AppDir = new FileInfo(Application.ResourceAssembly.Location).DirectoryName;

            try
            {
                LocalizationManager.ProbeDirectory(AppDir + "\\I18N");
            }
            catch (Exception) { }

            ServiceManager.ComposeAssemblyFromType(typeof(MainWindowViewModel));

            GlobalHanasuCore.Initialize(new Func<string, object, object>(HandleEvents),
                AppDir + "\\Plugins\\");

            if (GlobalHanasuCore.Plugins.Players != null)
                IsMuted = false;

            CurrentVolume = 50;

            GlobalHanasuCore.SetVolume(CurrentVolume);

            //LocalizationManager.ProbeDirectory

            SelectedStation = new Station();

            UIPanelState = FadeablePanelState.UpperFocus;

            IsPlaying = false;

            NeedsStationStreamSelection = false;

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

            VolumeMuteButtonCommand = new CrystalCommand(this, true, (o) =>
            {
                IsMuted = !IsMuted;
            });


            MediaFastForwardCommand = new NullCommand();
            MediaRewindCommand = new NullCommand();


            InitializeHTTPd();

            Application.Current.Exit += new ExitEventHandler(Current_Exit);
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            GlobalHanasuCore.Shutdown();
        }

        #region HTTPd
        private void InitializeHTTPd()
        {
            try
            {
                //Hanasu.Misc.HTTPd.HTTPdService.HttpPostReceived += HTTPdService_HttpPostReceived;
                HTTPdService.HttpUrlHandler += HTTPdService_HttpUrlHandler;

                //TODO: make the help data localizable
                HTTPdService.RegisterUrlHandler("/api", HTTPdService.HttpRequestType.GET, "Reports all of the commands that are registered in Hanasu.");
                HTTPdService.RegisterUrlHandler("/getlocalizedvalue", HTTPdService.HttpRequestType.GET, "Gets the localized vaule from the specified key. I.E. /getlocalizedvalue?key=Welcome");
                HTTPdService.RegisterUrlHandler("/getstationstreams", HTTPdService.HttpRequestType.GET, "Gets the stream(s) of a station and returns it in XML. I.E. /getstationstreams?station=XAMFM");
                HTTPdService.RegisterUrlHandler("/isplaying", HTTPdService.HttpRequestType.GET, "Gets if Hanasu is playing or not. Returns 'true' or 'false'.");
                HTTPdService.RegisterUrlHandler("/nowplaying", HTTPdService.HttpRequestType.GET, "Gets what the current song is from Hanasu.");
                HTTPdService.RegisterUrlHandler("/nowstation", HTTPdService.HttpRequestType.GET, "Gets what the current station is from Hanasu. See <a href=\"#stations\">/stations</a> for details.");
                HTTPdService.RegisterUrlHandler("/ping", HTTPdService.HttpRequestType.GET, "Sends PONG back to the sender.");
                HTTPdService.RegisterUrlHandler("/play", HTTPdService.HttpRequestType.POST, "Tells Hanasu to start playing the previously selected station.");
                HTTPdService.RegisterUrlHandler("/play2", HTTPdService.HttpRequestType.POST, "Tells Hanasu what station to play along with optional direct url. For example: /play2?station=XAMFM&url=http://173.192.205.178:80");
                HTTPdService.RegisterUrlHandler("/pause", HTTPdService.HttpRequestType.POST, "Tells Hanasu to stop playing the previously selected station.");
                HTTPdService.RegisterUrlHandler("/stations", HTTPdService.HttpRequestType.GET, "Gets the Hanasu stations catalog xml data.");

                HTTPdService.Start();
            }
            catch (Exception)
            {
            }
        }

        private Lazy<string> stationsXml = new Lazy<string>(() =>
            {
                return Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(GlobalHanasuCore.StationsService.StationsUrl);
            });

        object HTTPdService_HttpUrlHandler(string relativeUrl, Misc.HTTPd.HTTPdService.HttpRequestType type, string[] queryVars, string[] postdata)
        {
            if (type == Misc.HTTPd.HTTPdService.HttpRequestType.POST)
            {
                return Dispatcher.Invoke(new EmptyReturnDelegate(() =>
                        {
                            switch (relativeUrl.ToLower())
                            {
                                case "/play":
                                    if (MediaPlayCommand.CanExecute(null))
                                        MediaPlayCommand.Execute(null);
                                    break;
                                case "/pause":
                                    if (MediaStopCommand.CanExecute(null))
                                        MediaStopCommand.Execute(null);
                                    break;
                                case "/play2":
                                    {
                                        try
                                        {
                                            if (queryVars.Length == 0 || queryVars == null) return string.Empty;

                                            var station = queryVars.First(x => x.ToLower().StartsWith("station")).Split('=')[1];

                                            if (station == null) return string.Empty;

                                            string url = null;
                                            if (queryVars.Length >= 2)
                                            {
                                                url = queryVars.First(x => x.ToLower().StartsWith("url")).Split('=')[1];
                                            }

                                            SelectedStation = CatalogStations.First(t => t.Name.ToLower() == station.ToLower());

                                            if (url == null)
                                                PlaySelectedStation(SelectedStation);
                                            else
                                                PlayStationWithDirectUrl(SelectedStation, url);

                                        }
                                        catch (Exception)
                                        {
                                            return string.Empty;
                                        }

                                    }
                                    break;

                            }

                            return string.Empty;
                        }));
            }
            else if (type == Misc.HTTPd.HTTPdService.HttpRequestType.GET)
            {
                switch (relativeUrl.ToLower())
                {
                    case "/nowplaying":
                        if (GlobalHanasuCore.CurrentSong != null)
                            return GlobalHanasuCore.CurrentSong.ToSongString();
                        else
                            return "Nothing";
                    case "/nowstation":
                        if (GlobalHanasuCore.CurrentStation != null)
                            return GlobalHanasuCore.CurrentStation.Name;
                        else
                            return "Nothing";
                    case "/api":
                        {
                            #region Generates API doc
                            StringBuilder sb = new StringBuilder(); //its going to get messy

                            sb.AppendLine("<html>");
                            sb.AppendLine("\t<head>");
                            sb.AppendLine("\t\t<title>Hanasu Web API</title>"); //TODO: Localize
                            sb.AppendLine("\t</head>");
                            sb.AppendLine("\t<body>");

                            sb.AppendLine("\t\t<ul>");


                            foreach (var handler in HTTPdService.GetUrlHandlers())
                            {
                                sb.AppendLine("\t\t\t<li>");
                                sb.AppendLine("\t\t\t\t<a id=\"" + handler.Key.TrimStart('/') + "\"><h2>" + handler.Key + "</h2></a>");
                                sb.AppendLine("\t\t\t\t<h4>Http Method: " +
                                    Enum.GetName(typeof(Hanasu.Misc.HTTPd.HTTPdService.HttpRequestType), handler.Value.Item1));
                                sb.AppendLine("\t\t\t\t<h4>" + handler.Value.Item2 + "</h4>");
                                sb.AppendLine("\t\t\t</li>");
                            }

                            sb.AppendLine("\t\t</ul>");
                            sb.AppendLine("\t</body>");
                            sb.AppendLine("</html>");

                            return sb.ToString();
                            #endregion
                        }
                    case "/getlocalizedvalue":
                        {
                            if (queryVars.Length == 0)
                                return string.Empty;

                            var keyBit = queryVars[0].Split('=');

                            var key = keyBit[0].ToLower();

                            if (key == "key")
                                try
                                {
                                    return LocalizationManager.GetLocalizedValue(keyBit[1]);
                                }
                                catch (Exception)
                                {
                                    break;
                                }

                            return string.Empty;
                        }
                    case "/getstationstreams":
                        {
                            if (queryVars.Length == 0)
                                return string.Empty;

                            var stationBit = queryVars[0].Split('=');

                            var station = stationBit[0].ToLower();

                            if (station == "station")
                                try
                                {
                                    var stationValue = Hanasu.Core.Utilities.HtmlTextUtility.UrlDecode(stationBit[1]);

                                    var stat = CatalogStations.First(t => t.Name.ToLower() == stationValue.ToLower());

                                    var entries = GlobalHanasuCore.PreprocessForUrls(stat);

                                    if (entries == null && stat.ExplicitExtension != null)
                                        break;

                                    XDocument doc = new XDocument(
                                        new XDeclaration("1.0", "Unicode", "yes"),
                                        new XElement("Station",
                                            new XElement("Name", stat.Name),
                                            new XElement("Streams",
                                                entries.Length == 0
                                                ? new XElement[]
                                                    {
                                                        new XElement("Stream",
                                                            new XElement("Title", stat.Name),
                                                            new XElement("Url", stat.DataSource))
                                                    }
                                                : entries.Select(x =>
                                                    new XElement("Stream",
                                                        new XElement("Title", x.Title),
                                                        new XElement("Url", x.File))))));

                                    return doc.ToString();
                                }
                                catch (Exception)
                                {
                                    break;
                                }

                            return string.Empty;
                        }
                        break;
                    case "/isplaying":
                        return IsPlaying.ToString().ToLower();
                    case "/stations":
                        return stationsXml.Value;
                    case "/ping":
                        return "pong";
                }
            }

            return string.Empty;
        }

        void HTTPdService_HttpPostReceived(string file, object postdata)
        {
            //no longer used

            Dispatcher.BeginInvoke(new EmptyDelegate(() =>
                {
                    switch (file.ToLower())
                    {
                        case "/play":
                            if (MediaPlayCommand.CanExecute(null))
                                MediaPlayCommand.Execute(null);
                            break;
                        case "/pause":
                            if (MediaStopCommand.CanExecute(null))
                                MediaStopCommand.Execute(null);
                            break;
                    }
                }));
        }
        #endregion


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

        public CrystalCommand VolumeMuteButtonCommand { get; set; }
        public CrystalCommand VolumeLowButtonCommand { get; set; }
        public CrystalCommand VolumeMidButtonCommand { get; set; }
        public CrystalCommand VolumeHighButtonCommand { get; set; }

        public bool IsMuted
        {
            get
            {
                return (bool)this.GetPropertyOrDefaultType<bool>("IsMuted");
            }
            set
            {
                this.SetProperty("IsMuted", value);

                try
                {
                    GlobalHanasuCore.IsMuted = value;
                }
                catch (Exception)
                {
                }
            }
        }

        public int CurrentVolume
        {
            get
            {
                return (int)this.GetPropertyOrDefaultType<int>("CurrentVolume");
            }
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
                        Messenger.PushMessage(this, "StationTitleUpdated", data);
                        break;
                    }
                case GlobalHanasuCore.SongTitleUpdated:
                    {
                        Messenger.PushMessage(this, "SongTitleUpdated", data);
                        break;
                    }
                case GlobalHanasuCore.NowPlayingStatus:
                    {
                        IsPlaying = (bool)data;
                        break;
                    }
                case GlobalHanasuCore.NowPlayingReset:
                    {
                        Messenger.PushMessage(this, "NowPlayingReset");

                        if (GlobalHanasuCore.CurrentStation.StationType == StationType.Radio)
                            UIBackPanelView = null;
                        break;
                    }
                case GlobalHanasuCore.StationBufferingStatusChanged:
                    {
                        Messenger.PushMessage(this, "BufferingStatus", data);

                        break;
                    }
                case GlobalHanasuCore.StationMessagePushed:
                    {
                        NotificationsService.AddNotification(
                            GlobalHanasuCore.CurrentStation.StationType == StationType.Radio
                                ? LocalizationManager.GetLocalizedValue("RadioMessageHeader")
                                : LocalizationManager.GetLocalizedValue("TVMessageHeader"),
                            data.ToString());
                        break;
                    }
                case GlobalHanasuCore.CoreWarningPushed:
                    {
                        ServiceManager.Resolve<IMessageBoxService>()
                            .ShowMessage("Internal Error", data.ToString());
                        break;
                    }
                case GlobalHanasuCore.StationConnectionError:
                    {
                        //MahApps.Metro.Behaviours.
                        //TODO: Show some sort of error dialog.

                        if (data is string)
                            ServiceManager.Resolve<IMessageBoxService>()
                            .ShowMessage("Connection Error", "Unable to stream from station:" + Environment.NewLine + data);
                        else
                        {
                            Exception ex = (Exception)data;

                            ServiceManager.Resolve<IMessageBoxService>()
                                .ShowMessage("Connection Error", "Unable to stream from station:" + Environment.NewLine + ex.Message);
                        }
                        break;
                    }
                case GlobalHanasuCore.CoreDispatcherInvoke:
                    {
                        if (Dispatcher != null)
                            Dispatcher.Invoke(new EmptyDelegate(() =>
                                {
                                    ((Action)data).Invoke();
                                }));
                        else
                            try
                            {
                                ((Action)data).Invoke();
                            }
                            catch (Exception)
                            {
                            }

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
                case GlobalHanasuCore.PlayerDetectedStationTypeDetected:
                    {
                        Messenger.PushMessage(this, "PlayerDetectedStationTypeDetected", data);
                        break;
                    }
                case GlobalHanasuCore.StationMultipleServersFound:
                    {
                        //Deal with choosing multiple stations.

                        //NeedsStationStreamSelection = true;

                        Tuple<Station, IMultiStreamEntry[]> info = (dynamic)data;

                        IMultiStreamEntry[] entries = info.Item2;

                        Tuple<bool, IMultiStreamEntry> res = null;

                        ChooseStationStreamWindow cssw = new ChooseStationStreamWindow();

                        Messenger.PushMessage(this, "StationStreamWindowStationPushed", info.Item1);
                        Messenger.PushMessage(this, "StationStreamWindowStreamsPushed", entries);

                        cssw.Owner = Application.Current.MainWindow;

                        res = new Tuple<bool, IMultiStreamEntry>(false, null);

                        Task.Factory.StartNew(() =>
                        {
                            var dat = (IMultiStreamEntry)Messenger.WaitForMessage("StationStreamChoosen").Data;
                            lock (res = new Tuple<bool, IMultiStreamEntry>(dat != null, dat))
                            {
                            }
                        }).ContinueWith(t => t.Dispose());


                        var dResult = cssw.ShowDialog();

                        if (dResult != true && res.Item2 == null)
                            res = new Tuple<bool, IMultiStreamEntry>(false, null);
                        else
                        {
                            Thread.Sleep(75); //May add this back if problems arise.
                        }

                        cssw.Close();

                        //NeedsStationStreamSelection = false;

                        return res;
                    }
            }

            return null;
        }

        public bool NeedsStationStreamSelection
        {
            get { return (bool)this.GetProperty("NeedsStationStreamSelection"); }
            set { this.SetProperty("NeedsStationStreamSelection", value); }
        }

        private void PlaySelectedStation(object o)
        {

            if (o == null) return;

            var stat = (Station)o;

            if (stat.Name == null) return;

            GlobalHanasuCore.StopStation();

            GlobalHanasuCore.PlayStation(stat);

            GlobalHanasuCore.SetVolume(CurrentVolume);

            if (stat.Logo != null)
                Messenger.PushMessage(this, "DisplayStationLogo", stat.Logo);
        }

        private void PlayStationWithDirectUrl(Station stat, string url)
        {
            if (url == null) return;

            if (stat.Name == null) return;

            GlobalHanasuCore.StopStation();

            GlobalHanasuCore.PlayStation(stat, url);

            GlobalHanasuCore.SetVolume(CurrentVolume);

            if (stat.Logo != null)
                Messenger.PushMessage(this, "DisplayStationLogo", stat.Logo);
        }

        private void StopSelectedStation(object o)
        {
            GlobalHanasuCore.StopStation();
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
        }
    }
}
