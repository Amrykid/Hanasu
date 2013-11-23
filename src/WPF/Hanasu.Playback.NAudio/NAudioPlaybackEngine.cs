using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Hanasu.Extensibility;

namespace Hanasu.Playback.NAudio
{
    [Export(typeof(IPlaybackEngine))]
    public class NAudioPlaybackEngine: IPlaybackEngine
    {
        private IAudioStreamer streamer = null;

        public void Play(string url)
        {
            if (streamer != null)
            {
                streamer.Dispose();
            }

            streamer = new ShoutcastAudioStreamer(x =>
                {
                    var d = x;
                });

            streamer.Stream(url);
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<PlaybackMetaDataChangedEventArgs> MetadataChanged;

        public event EventHandler PlaybackStatusChanged;
    }
}
