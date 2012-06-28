using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hanasu.Core;
using Hanasu.Services.Events;

namespace Hanasu.Services.Settings
{
    public class SettingsService : BaseINPC, IStaticService
    {
        static SettingsService()
        {
            Instance = new SettingsService();
        }
        public static SettingsService Instance { get; private set; }
        public static void Initialize()
        {
            Instance.InitializeImpl();
        }
        private void InitializeImpl()
        {
            System.Windows.Application.Current.Exit += Current_Exit;

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\";

            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            SettingsFilepath = dir + "settings.xml";

            if (!System.IO.File.Exists(SettingsFilepath))
            {
                CreateNewSettingsFile();
            }
            else
            {
                //It exist. Load it

                var doc = XDocument.Load(SettingsFilepath);
                var settings = (doc.LastNode as XElement);
                var songfetch = settings.Element("AutomaticallyFetchSongData");

                AutomaticallyFetchSongData = bool.Parse(songfetch.Value);

                var liveupdate = settings.Element("UpdateStationsLive");

                UpdateStationsLive = bool.Parse(liveupdate.Value);

                LastSetVolume = settings.ContainsElement("LastSetVolume") ? int.Parse(settings.Element("LastSetVolume").Value) : 15;

                var sinfo = new SettingsDataEventInfo()
                {
                    SettingsDocument = doc,
                    SettingsElement = settings
                };

                EventService.RaiseEvent(EventType.Settings_Loaded, sinfo);
            }
        }

        internal void CreateNewSettingsFile(string file = null)
        {
            //Create the settings xml

            var doc = new XDocument(
                new XDeclaration("1.0", "Unicode", "yes"));

            var settings = new XElement("Settings",
                    new XElement("UpdateStationsLive", "false"),
                    new XElement("AutomaticallyFetchSongData", "false"),
                    new XElement("LastSetVolume", 50));

            //Pass the settings to subscribers so they can update the settings xml as needed.
            var sinfo = new SettingsDataEventInfo()
            {
                SettingsDocument = doc,
                SettingsElement = settings
            };

            EventService.RaiseEvent(EventType.Settings_Created, sinfo);

            settings = sinfo.SettingsElement;

            UpdateStationsLive = false;
            AutomaticallyFetchSongData = false;

            doc.Add(settings);

            doc.Save(file == null ? SettingsFilepath : file);
        }

        void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            XDocument doc = new XDocument(
                     new XDeclaration("1.0", "Unicode", "yes"));

            var settings = new XElement("Settings",
                          new XElement("UpdateStationsLive", UpdateStationsLive.ToString()),
                          new XElement("AutomaticallyFetchSongData", AutomaticallyFetchSongData.ToString()),
                          new XElement("LastSetVolume", LastSetVolume));

            var sinfo = new SettingsDataEventInfo()
            {
                SettingsDocument = doc,
                SettingsElement = settings
            };

            EventService.RaiseEvent(EventType.Settings_Saving, sinfo);

            settings = sinfo.SettingsElement;

            doc.Add(settings);

            doc.Save(SettingsFilepath);
        }
        public string SettingsFilepath { get; private set; }

        public bool AutomaticallyFetchSongData { get; set; }
        public bool UpdateStationsLive { get; set; }

        public int LastSetVolume { get; set; }


        internal class SettingsDataEventInfo : EventInfo
        {
            public XDocument SettingsDocument { get; set; }
            public XElement SettingsElement { get; set; }
        }
    }
}
