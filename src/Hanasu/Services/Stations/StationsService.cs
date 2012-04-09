using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Core;

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
            Instance = new StationsService();
        }
        public static StationsService Instance { get; private set; }

        public StationsService()
        {
            Stations = new ObservableCollection<Station>();

            //Dummy test
            //Will move these to an external file soon
            Stations.Add(
                new Station()
                {
                    City = "Tokyo",
                    DataSource = new Uri("http://www.animenfo.com/radio/listen.m3u"),
                    Format = RadioFormat.Mix,
                    Homepage = new Uri("http://www.animenfo.com/"),
                    Name = "AnimeNfo"
                });

            Stations.Add(
               new Station()
               {
                   City = "Tokyo",
                   DataSource = new Uri("http://stream.gensokyoradio.net:8000/listen.m3u?sid=1"),
                   Format = RadioFormat.Mix,
                   Homepage = new Uri("http://www.gensokyoradio.net/"),
                   Name = "Gensokyo Radio"
               });

            Stations.Add(
               new Station()
               {
                   City = "Tokyo",
                   DataSource = new Uri("http://www.makeavoice.com/shoutcast/tuneinlinks.php?file=m3u&host=67.215.229.100&port=8290"),
                   Format = RadioFormat.Mix,
                   Homepage = new Uri("http://nihonradio.net/2/"),
                   Name = "Nihon Radio"
               });

            OnPropertyChanged("Stations");
        }

        public ObservableCollection<Station> Stations { get; private set; }
    }
}

//http://www.surfmusic.de/country/japan.html