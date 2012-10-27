using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crystal.Core;
using Crystal.Localization;
using Hanasu.Misc.HTTPd;
using Hanasu.Core;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows;

namespace Hanasu.ViewModel
{
    public class MainWindowWebControlFlyoutViewModel : BaseViewModel
    {
        public MainWindowWebControlFlyoutViewModel()
        {
            this.BindManager.Bind("WebServerStatus", (s, e) =>
            {
                StatusVisual = WebServerStatus == true ? (Visual)Application.Current.MainWindow.Resources["appbar_check"] : (Visual)Application.Current.MainWindow.Resources["appbar_cancel"];
            });

            InitializeHTTPd();

            this.RaisePropertyChanged("WebServerStatus");
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
                HTTPdService.RegisterUrlHandler("/js/boot.js", HTTPdService.HttpRequestType.GET, "Not for general use.");
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

        object HTTPdService_HttpUrlHandler(string relativeUrl, Misc.HTTPd.HTTPdService.HttpRequestType type, string[] queryVars, string[] postdata, out string mimetype)
        {
            mimetype = HttpMimeTypes.Html;

            if (type == Misc.HTTPd.HTTPdService.HttpRequestType.POST)
            {
                #region POST
                return Dispatcher.Invoke(new EmptyReturnDelegate(() =>
                {
                    switch (relativeUrl.ToLower())
                    {
                        case "/play":
                            if (AppViewModel.MainViewModel.MediaPlayCommand.CanExecute(null))
                                AppViewModel.MainViewModel.MediaPlayCommand.Execute(null);
                            break;
                        case "/pause":
                            if (AppViewModel.MainViewModel.MediaStopCommand.CanExecute(null))
                                AppViewModel.MainViewModel.MediaStopCommand.Execute(null);
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

                                    AppViewModel.MainViewModel.SelectedStation = AppViewModel.MainViewModel.CatalogStations.First(t => t.Name.ToLower() == station.ToLower());

                                    if (url == null)
                                        AppViewModel.MainViewModel.PlaySelectedStation(AppViewModel.MainViewModel.SelectedStation);
                                    else
                                        AppViewModel.MainViewModel.PlayStationWithDirectUrl(AppViewModel.MainViewModel.SelectedStation, url);

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
                #endregion
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
                    case "/js/boot.js":
                        mimetype = HttpMimeTypes.Javascript;
                        return "// This will be changed automatically. Do not alter this file, humans." + Environment.NewLine + "var isWeb = false;";
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

                                    var stat = AppViewModel.MainViewModel.CatalogStations.First(t => t.Name.ToLower() == stationValue.ToLower());

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
                        return AppViewModel.MainViewModel.IsPlaying.ToString().ToLower();
                    case "/stations":
                        return stationsXml.Value;
                    case "/ping":
                        return "pong";
                }
            }

            return string.Empty;
        }
        #endregion

        public bool WebServerStatus
        {
            get { return HTTPdService.IsRunning; }
        }

        public Visual StatusVisual
        {
            get { return (Visual)this.GetProperty(x => this.StatusVisual); }
            set { this.SetProperty(x => this.StatusVisual, value); }
        }
    }
}
