using System;
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
using Crystal.Messaging;
using Crystal.Services;

namespace Hanasu.ViewModel
{
    public class StationsViewModel : BaseViewModel
    {
        Crystal.Messaging.MessageRelayedProperty<bool> IsBusyProperty = null;
        public StationsViewModel()
        {
            if (!IsDesignMode)
                Initialize();
            else
            {
                AvailableStations = new ObservableCollection<Station>();
                AvailableStations.Add(new Station() { Title = "AmryFM" });
            }
        }

        private async System.Threading.Tasks.Task Initialize()
        {
            IsBusyProperty = CreateRelayProperty<bool>("IsBusy", null);

            await Task.Delay(3000); //Wait a bit before loading.

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
            set { SetProperty(x => this.IsBusy, value); Messenger.PushMessage(this, "UpdateIsBusy", value); } //IsBusyProperty.Value = value; }
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

            try
            {
                XDocument doc = null;
                using (var http = new HttpClient())
                {
                    string data = await http.GetStringAsync("https://raw.github.com/Amrykid/Hanasu/master/MobileStations.xml").ConfigureAwait(false);
                    doc = await Task.Run(() => XDocument.Parse(data)).ConfigureAwait(false);
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

                await UIDispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var x in stations)
                        AvailableStations.Add(x);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            catch (HttpRequestException)
            {
                ServiceManager.Resolve<IMessageBoxService>().ShowMessage("Un-oh!", "Unable to retrieve available stations from the repository!");
            }
        }
    }
}
