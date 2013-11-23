using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Extensibility
{
    public class PlaybackMetaDataChangedEventArgs: EventArgs
    {
        public string Artist { get; set; }
        public string Track { get; set; }
    }
}
