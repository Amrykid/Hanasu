using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Core;
using System.Xml.Linq;
using System.Timers;
using Hanasu.Services.Logging;
using System.IO;
using System.Net;
using System.Xml;
using System.Windows;

namespace Hanasu.Services.Stations
{
    public class StationsService : BaseINPC, IStaticService
    {
        static StationsService()
        {
            if (Instance == null)
                Initialize();
        }
        public static void Initialize()
        {
            if (Instance == null)
                Instance = new StationsService();
        }
        public static StationsService Instance { get; private set; }

        private Timer timer = null; //Timer is used for 'streaming updates' of new radio stations. That way, new  stations are added without reloading Hanasu.

        public StationsService()
        {
            LogService.Instance.WriteLog(this,
                "Stations Service initialized.");

            Stations = new ObservableCollection<Station>();
            timer = new Timer();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\");

            StationsCacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\Cache\\";
            if (!Directory.Exists(StationsCacheDir))
                Directory.CreateDirectory(StationsCacheDir);



            System.Threading.Tasks.Task.Factory.StartNew(() =>
                LoadStationsFromRepo()).ContinueWith((tk) => tk.Dispose());

            if (Hanasu.Services.Settings.SettingsService.Instance.UpdateStationsLive)
            {
                timer.Elapsed += timer_Elapsed;

                timer.Interval = 60000 * 30; //30 minutes

                timer.Start();
            }
        }

        public string StationsCacheDir = null;
        public string StationsCachedFile { get { return StationsCacheDir + "Stations.xml"; } }
        public string StationsUrl { get { return "https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml"; } }

        internal void LoadStationsFromRepo()
        {
            try
            {
                LogService.Instance.WriteLog(this,
    "BEGIN: Station polling operation.");

                System.Windows.Application.Current.Dispatcher.Invoke(new EmptyParameterizedDelegate((t) =>
                    {
                        if (StationFetchStarted != null)
                            StationFetchStarted((StationsService)t, null);
                    }), this);

                Status = StationsServiceStatus.Polling;

                //System.Threading.Thread.Sleep(10000);

                System.Windows.Application.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        Stations.Clear();
                    }));


                //var doc = XDocument.Load("https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml");


                RadioFormat dummie = 0;
                dynamic stats = null;

                if (File.Exists(StationsCachedFile))
                {
                    try
                    {
                        var doc = XDocument.Load(StationsCachedFile);
                        stats = from x in doc.Element("Stations").Elements("Station")
                                select ParseStation(ref dummie, x);
                    }
                    catch (Exception)
                    {
                        if (NetworkUtils.IsConnectedToInternet())
                        {
                            stats = from x in StreamStationsXml()
                                    select ParseStation(ref dummie, x);
                        }
                        else
                        {
                            MessageBox.Show("Unable to load cached Stations file. Also, unable to connect to online Stations file. Hanasu will now exit.");
                            Application.Current.Shutdown();
                        }
                    }
                }
                else
                    if (NetworkUtils.IsConnectedToInternet())
                    {

                        stats = from x in StreamStationsXml()
                                select ParseStation(ref dummie, x);

                        DownloadStationsToCache();
                    }
                    else
                    {
                        MessageBox.Show("Unable to connect to online Stations file. Hanasu will now exit.");
                        Application.Current.Shutdown();
                    }


                var finalstats = new List<Station>();
                foreach (Station st in stats)
                {
                    //Checks if its possible to cache the playlist file.
                    var s = st;
                    if (st.ExplicitExtension != "" && Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(st.DataSource, st.ExplicitExtension) && st.Cacheable && st.StationType == StationType.Radio)
                    {
                        var cachefile = StationsCacheDir + st.Name + "_" + st.DataSource.LocalPath.Substring(st.DataSource.LocalPath.LastIndexOf("/") + 1);
                        if (!File.Exists(cachefile))
                            using (WebClient wc = new WebClient())
                            {
                                wc.DownloadFile(st.DataSource, cachefile);
                                s.LocalStationFile = new Uri(cachefile);
                            }
                        else
                            s.LocalStationFile = new Uri(cachefile);
                    }

                    finalstats.Add(s);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(new EmptyParameterizedDelegate2((f, g) =>
                    {
                        foreach (var x in (List<Station>)f)
                            Stations.Add(x);

                        OnPropertyChanged("Stations");

                        if (StationFetchCompleted != null)
                            StationFetchCompleted((StationsService)g, null);
                    }), finalstats, this);

                LogService.Instance.WriteLog(this,
    "END: Station polling operation.");

            }
            catch (Exception) { }

            Status = StationsServiceStatus.Idle;
        }

        internal void DownloadStationsToCache()
        {
            using (var wc = new WebClient())
            {
                wc.DownloadFileAsync(new Uri(StationsUrl), StationsCachedFile);
            }
        }
        internal System.Threading.Tasks.Task DownloadStationsToCacheAsync()
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(new Uri(StationsUrl), StationsCachedFile);
                    }
                }).ContinueWith(t => t.Dispose());
        }

        private static Station ParseStation(ref RadioFormat dummie, XElement x)
        {
            return new Station()
            {
                Name = x.Element("Name").Value,
                DataSource = new Uri(x.Element("DataSource").Value),
                Homepage = new Uri(x.Element("Homepage").Value),
                Format = (Enum.TryParse<RadioFormat>(x.Element("Format").Value, out dummie) == true ?
                    (RadioFormat)Enum.Parse(typeof(RadioFormat), x.Element("Format").Value) :
                    RadioFormat.Mix),
                City = x.Element("City").Value,
                ExplicitExtension = x.ContainsElement("ExplicitExtension") ? x.Element("ExplicitExtension").Value : null,
                StationType = x.ContainsElement("StationType") ? (StationType)Enum.Parse(typeof(StationType), x.Element("StationType").Value) : StationType.Radio,
                Language = x.ContainsElement("Language") ? (StationLanguage)Enum.Parse(typeof(StationLanguage), x.Element("Language").Value) : StationLanguage.English,
                Cacheable = x.ContainsElement("Cacheable") ? bool.Parse(x.Element("Cacheable").Value) : false
            };
        }

        private IEnumerable<XElement> StreamStationsXml()
        {
            using (XmlReader reader = XmlReader.Create("https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml"))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "Station")
                            {
                                XElement el = XElement.ReadFrom(reader)
                                                      as XElement;
                                if (el != null)
                                    yield return el;
                            }
                            break;
                    }
                }
                reader.Close();
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NetworkUtils.IsConnectedToInternet())
            {
                int before = Stations.Count;

                System.Windows.Application.Current.Dispatcher.Invoke(
                    new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                        {
                            LoadStationsFromRepo();
                        }));

                if (Stations.Count > before)
                {
                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Stations Updated",
                        (Stations.Count - before).ToString() + " station(s) added.", 4000, true);

                    DownloadStationsToCache();
                }
            }
        }

        private delegate void EmptyParameterizedDelegate(object obj);
        private delegate void EmptyParameterizedDelegate2(object obj, object obj2);

        public ObservableCollection<Station> Stations { get; private set; }

        public StationsServiceStatus Status { get; private set; }

        public event EventHandler StationFetchStarted;
        public event EventHandler StationFetchCompleted;
    }
}

//http://www.surfmusic.de/country/japan.html