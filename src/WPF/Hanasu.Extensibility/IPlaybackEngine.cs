using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Extensibility
{
    public interface IPlaybackEngine
    {
        void Play(string url);
        void Resume();
        void Pause();
        void Stop();
        void Shutdown();

        float Volume { get; set; }

        bool IsPlaying { get; }

        event EventHandler<PlaybackMetaDataChangedEventArgs> MetadataChanged;
        event EventHandler PlaybackStatusChanged;
    }
}
