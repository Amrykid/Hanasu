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
            
            OnPropertyChanged("Stations");
        }

        public ObservableCollection<Station> Stations { get; private set; }
    }
}

//http://www.surfmusic.de/country/japan.html