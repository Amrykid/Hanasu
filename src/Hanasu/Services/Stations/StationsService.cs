using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Core;
using System.Xml.Linq;
using System.Timers;

namespace Hanasu.Services.Stations
{
    public class StationsService : BaseINPC
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
            Stations = new ObservableCollection<Station>();
            timer = new Timer();

            timer.Elapsed += timer_Elapsed;

            timer.Interval = 60000 * 2; //2 minutes

            System.Threading.Tasks.Task.Factory.StartNew(() =>
                LoadStationsFromRepo()).ContinueWith((tk) => tk.Dispose());

            timer.Start();
        }

        private void LoadStationsFromRepo()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        if (StationFetchStarted != null)
                            StationFetchStarted(this, null);
                    }));

                Status = StationsServiceStatus.Polling;

                //System.Threading.Thread.Sleep(10000);

                var doc = XDocument.Load("https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml");

                System.Windows.Application.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        Stations.Clear();
                    }));

                RadioFormat dummie = 0;

                var stats = from x in doc.Element("Stations").Elements("Station")
                            select new Station()
                            {
                                Name = x.Element("Name").Value,
                                DataSource = new Uri(x.Element("DataSource").Value),
                                Homepage = new Uri(x.Element("Homepage").Value),
                                Format = (Enum.TryParse<RadioFormat>(x.Element("Format").Value, out dummie) == true ? 
                                    (RadioFormat)Enum.Parse(typeof(RadioFormat), x.Element("Format").Value) : 
                                    RadioFormat.Mix),
                                City = x.Element("City").Value,
                            };


                System.Windows.Application.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        foreach (var x in stats)
                            Stations.Add(x);

                        OnPropertyChanged("Stations");

                        if (StationFetchCompleted != null)
                            StationFetchCompleted(this, null);
                    }));

            }
            catch (Exception) { }

            Status = StationsServiceStatus.Idle;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int before = Stations.Count;

            System.Windows.Application.Current.Dispatcher.Invoke(
                new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        LoadStationsFromRepo();
                    }));

            if (Stations.Count > before)
                Hanasu.Services.Notifications.NotificationsService.AddNotification("Stations Updated",
                    (Stations.Count - before).ToString() + " station(s) added.", 4000,true);
        }

        public ObservableCollection<Station> Stations { get; private set; }

        public StationsServiceStatus Status { get; private set; }

        public event EventHandler StationFetchStarted;
        public event EventHandler StationFetchCompleted;
    }
}

//http://www.surfmusic.de/country/japan.html