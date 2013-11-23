using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hanasu.Extensibility;
using Hanasu.Model;

namespace Hanasu
{
    public static class PlaybackEngine
    {
        public static IPlaybackEngine Engine { get; private set; }

        public static void Initialize()
        {
            /// would use MEF but...
            /// 
            Engine = new Hanasu.Playback.FMODPlayback.FMODAudioPlaybackEngine();

            StartPlaying("http://173.192.205.178:80");
        }

        public static void StartPlaying(Station station)
        {
            //check if preprocessing is needed?
            //engine.Play(station.StreamUrl);
        }
        public static void StartPlaying(string url)
        {
            Engine.Play(url);
        }
    }
}
