using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Hanasu.Core;
using System.Xml.Linq;
using System.Timers;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using Hanasu.Core.Utilities;
using System.Threading.Tasks;

namespace Hanasu.Core.Stations
{
    public class StationsService
    {
        internal StationsService()
        {
            Stations = new ObservableCollection<Station>();
            CustomStations = new ObservableCollection<Station>();

        }
        public string StationsUrl { get { return "https://raw.github.com/Amrykid/Hanasu/master/src/Hanasu/Stations.xml"; } }

        internal Task LoadStationsFromRepoAsync()
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
                LoadStationsFromRepo());
        }

        internal void LoadStationsFromRepo()
        {
            Stations.Clear();

            RadioFormat dummie = 0;
            dynamic stats = from x in StreamStationsXml()
                            select ParseStation(ref dummie, x);

            foreach (var stat in stats)
                Stations.Add(stat);
        }

        private Station ParseStation(ref RadioFormat dummie, XElement x)
        {
            return new Station()
            {
                Name = x.Element("Name").Value,
                DataSource = new Uri(x.Element("DataSource").Value),
                Homepage = string.IsNullOrEmpty(x.Element("Homepage").Value) ? null : new Uri(x.Element("Homepage").Value),
                Format = (Enum.TryParse<RadioFormat>(x.Element("Format").Value, out dummie) == true ?
                    (RadioFormat)Enum.Parse(typeof(RadioFormat), x.Element("Format").Value) :
                    RadioFormat.Mix),
                City = x.Element("City").Value,
                ExplicitExtension = x.ContainsElement("ExplicitExtension") ? x.Element("ExplicitExtension").Value : null,
                StationType = x.ContainsElement("StationType") ? (StationType)Enum.Parse(typeof(StationType), x.Element("StationType").Value) : StationType.Radio,
                Language = x.ContainsElement("Language") ? (StationLanguage)Enum.Parse(typeof(StationLanguage), x.Element("Language").Value) : StationLanguage.Unknown,
                Cacheable = x.ContainsElement("Cacheable") ? bool.Parse(x.Element("Cacheable").Value) : false,
                ScheduleType = x.ContainsElement("Schedule") ? (StationScheduleType)Enum.Parse(typeof(StationScheduleType), x.Element("Schedule").Attribute("type").Value) : StationScheduleType.none,
                ScheduleUrl = x.ContainsElement("Schedule") ? (string.IsNullOrEmpty(x.Element("Schedule").Value) ? null : new Uri(x.Element("Schedule").Value)) : null,
                Logo = x.ContainsElement("Logo") ? (string.IsNullOrEmpty(x.Element("Logo").Value) ? null : new Uri(x.Element("Logo").Value)) : null,
                UseAlternateSongTitleFetching = x.ContainsElement("UseAlternateSongTitleFetching") ? bool.Parse(x.Element("UseAlternateSongTitleFetching").Value) : false,
                TimeZoneInfo = x.ContainsElement("Timezone") ? TimeZoneInfo.FindSystemTimeZoneById(x.Element("Timezone").Value) : null
            };
        }

        private IEnumerable<XElement> StreamStationsXml()
        {
            using (XmlReader reader = XmlReader.Create(StationsUrl))
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

        private delegate void EmptyParameterizedDelegate(object obj);
        private delegate void EmptyParameterizedDelegate2(object obj, object obj2);

        internal ObservableCollection<Station> Stations { get; private set; }
        internal ObservableCollection<Station> CustomStations { get; private set; }
    }
}

//http://www.surfmusic.de/country/japan.html