using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;

namespace Hanasu.Services.Stations
{
    public class Station
    {
        public string Name { get; set; }
        public Uri Homepage { get; set; }
        public Uri DataSource { get; set; }
        public string City { get; set; }
        public RadioFormat Format { get; set; }
        public string ExplicitExtension { get; set; }
        public StationType StationType { get; set; }
        public StationLanguage Language { get; set; }
    }
}
