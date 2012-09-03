using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;

namespace Hanasu.Core.Stations
{
    [Serializable]
    public struct Station
    {
        public string Name { get; set; }
        public Uri Homepage { get; set; }
        public Uri DataSource { get; set; }
        public string City { get; set; }
        public RadioFormat Format { get; set; }
        public string ExplicitExtension { get; set; }
        public StationType StationType { get; set; }
        public StationLanguage Language { get; set; }

        public Uri LocalStationFile { get; set; }
        public bool Cacheable { get; set; }

        public Uri ScheduleUrl { get; set; }
        public StationScheduleType ScheduleType { get; set; }

        public Uri Logo { get; set; }

        public bool UseAlternateSongTitleFetching { get; set; }

        public string PreferredStoreSource { get; set; }
        public string PreferredLyricsSource { get; set; }

        public TimeZoneInfo TimeZoneInfo { get; set; }

        public StationServerType ServerType { get; set; }

        public static bool operator ==(Station s1, Station s2)
        {
            return s1.Equals(s2);

            //return s1.Cacheable == s2.Cacheable
            //    && s1.City == s2.City
            //    && s1.DataSource == s2.DataSource
            //    && s1.ExplicitExtension == s2
        }

        public static bool operator !=(Station s1, Station s2)
        {
            return !s1.Equals(s2);
        }
    }
}
