using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Playback.NAudio
{
    internal interface IAudioStreamer: IDisposable
    {
        void Stream(string url);
    }
}
