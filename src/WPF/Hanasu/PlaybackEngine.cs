﻿using System;
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
            Engine.MetadataChanged += Engine_MetadataChanged;

            Task.Delay(1000)
                .ContinueWith(x =>
            {
                Engine.Volume = 1.0F;
            });
            StartPlaying("http://173.192.205.178:80");
            //StartPlaying("http://itori.animenfo.com:443/;");
        }

        static void Engine_MetadataChanged(object sender, PlaybackMetaDataChangedEventArgs e)
        {
            CurrentArtist = e.Artist;
            CurrentTrack = e.Track;
        }

        public static string CurrentTrack { get; private set; }
        public static string CurrentArtist { get; private set; }

        public static void StartPlaying(Station station)
        {
            //check if preprocessing is needed?
            //engine.Play(station.StreamUrl);
        }
        public static void StartPlaying(string url)
        {
            Engine.Play(url);
        }

        public static void Shutdown()
        {
            if (Engine != null)
            {
                Engine.MetadataChanged -= Engine_MetadataChanged;

                Engine.Stop();

                Engine.Shutdown();
            }
        }
    }
}
