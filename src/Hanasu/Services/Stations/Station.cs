using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public object StationTypeCanvas { get { return new Hanasu.Core.Converters.VisualStationTypeConverter().Convert(StationType, null, null, null); } }
    }
}
