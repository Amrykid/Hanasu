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

            LoadStationsFromRepo();

            timer.Start();
        }

        private void LoadStationsFromRepo()
        {
            try
            {
                var doc = XDocument.Load("https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml");

                Stations.Clear();

                var stats = from x in doc.Element("Stations").Elements("Station")
                            select new Station()
                            {
                                Name = x.Element("Name").Value,
                                DataSource = new Uri(x.Element("DataSource").Value),
                                Homepage = new Uri(x.Element("Homepage").Value),
                                Format = (RadioFormat)Enum.Parse(typeof(RadioFormat), x.Element("Format").Value),
                                City = x.Element("City").Value,
                            };

                foreach (var x in stats)
                    Stations.Add(x);

            }
            catch (Exception) { }
            OnPropertyChanged("Stations");
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
                    (Stations.Count - before).ToString() + " station(s) added.", 4000);
        }

        public ObservableCollection<Station> Stations { get; private set; }
    }
}

//http://www.surfmusic.de/country/japan.html