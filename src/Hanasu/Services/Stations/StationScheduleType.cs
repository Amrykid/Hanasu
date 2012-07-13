using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Stations
{
    [Serializable]
    public enum StationScheduleType
    {
        none = 0,
        page = 1,
        image = 2,
        ics = 3
    }
}
