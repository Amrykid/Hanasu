﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crystal.Core;
using Hanasu.Model;
using Hanasu.Extensions;
using Crystal.Localization;

namespace Hanasu.ViewModel
{
    public class StationsViewModel: BaseViewModel
    {
        public StationsViewModel()
        {
            Initialize();
        }

        private async System.Threading.Tasks.Task Initialize()
        {
            await DoWork(LoadStations());
        }

        void DoWork(Action action)
        {
            IsBusy = true;
            action();
            IsBusy = false;
        }
        async Task DoWork(Task action)
        {
            IsBusy = true;
            await action;
            IsBusy = false;
        }

        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty(x => this.IsBusy, value); }
        }

        public Station SelectedStation
        {
            get { return GetPropertyOrDefaultType<Station>(x => this.SelectedStation); }
            set { SetProperty(x => this.SelectedStation, value); }
        }

        public ObservableCollection<Station> AvailableStations
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<Station>>(x => this.AvailableStations); }
            set { SetProperty(x => this.AvailableStations, value); }
        }

        internal async Task LoadStations()
        {
            if (AvailableStations == null)
                AvailableStations = new ObservableCollection<Station>();
            else
                if (AvailableStations.Count > 0)
                    AvailableStations.Clear();

            XDocument doc = null;
            using (var http = new HttpClient())
            {
                doc = XDocument.Parse(await http.GetStringAsync("https://raw.github.com/Amrykid/Hanasu/master/MobileStations.xml"));
            }


            var stationsElement = doc.Element("Stations");

            var stations = from x in stationsElement.Elements("Station")
                           where x.ContainsElement("StationType") ? x.Element("StationType").Value != "TV" : true
                           select new Station()
                           {
                               Title = x.Element("Name").Value,
                               StreamUrl = x.Element("DataSource").Value,
                               PreprocessorFormat = x.ContainsElement("ExplicitExtension") ? x.Element("ExplicitExtension").Value : string.Empty,
                               ImageUrl = x.ContainsElement("Logo") ? x.Element("Logo").Value : "http://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/200px-No_image_available.svg.png",
                               UnlocalizedFormat = x.Element("Format").Value,
                               Format = x.Element("Format").Value,
                               Subtitle = LocalizationManager.GetLocalizedValue("StationSubtitle"),
                               ServerType = x.ContainsElement("ServerType") ? x.Element("ServerType").Value : "Raw",
                               HomepageUrl = x.ContainsElement("Homepage") ? new Uri(x.Element("Homepage").Value) : null
                           };

            foreach (var x in stations)
                AvailableStations.Add(x);
        }
    }
}
