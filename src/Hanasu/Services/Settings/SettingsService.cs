using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hanasu.Core;

namespace Hanasu.Services.Settings
{
    public class SettingsService : BaseINPC
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
                //Create the settings xml

                var doc = new XDocument(
                    new XDeclaration("1.0", "Unicode", "yes"),
                    new XElement("Settings",
                        new XElement("UpdateStationsLive","false"),
                        new XElement("AutomaticallyFetchSongData", "false")));

                UpdateStationsLive = false;
                AutomaticallyFetchSongData = false;

                doc.Save(SettingsFilepath);
            }
            else
            {
                //It exist. Load it

                var doc = XDocument.Load(SettingsFilepath);
                var songfetch = (doc.LastNode as XElement).Element("AutomaticallyFetchSongData");

                AutomaticallyFetchSongData = bool.Parse(songfetch.Value);
            }
        }

        void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            var doc = new XDocument(
                    new XDeclaration("1.0", "Unicode", "yes"),
                    new XElement("Settings",
                        new XElement("UpdateStationsLive",UpdateStationsLive.ToString()),
                        new XElement("AutomaticallyFetchSongData", AutomaticallyFetchSongData.ToString())));


            doc.Save(SettingsFilepath);
        }
        public string SettingsFilepath { get; private set; }

        public bool AutomaticallyFetchSongData { get; set; }
        public bool UpdateStationsLive { get; set; }
    }
}
