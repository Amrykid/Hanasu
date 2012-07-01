using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Events
{
    public enum EventType
    {
        Station_Changed = 1,
        Song_Liked = 2,
        Settings_Loaded = 3,
        Settings_Saving = 4,
        Settings_Created = 5,
        Theme_Changed = 6
    }
}
