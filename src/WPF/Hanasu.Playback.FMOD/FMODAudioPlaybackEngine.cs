using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Hanasu.Extensibility;

namespace Hanasu.Playback.FMODPlayback
{
    [Export(typeof(IPlaybackEngine))]
    public class FMODAudioPlaybackEngine : IPlaybackEngine
    {
        private FMOD.System system = null;
        private FMOD.Sound sound = null;
        private FMOD.Channel channel = null;
        private bool soundcreated = false;

        private System.Timers.Timer timer = null;
        private string _title, _artist = null;

        public FMODAudioPlaybackEngine()
        {
            uint version = 0;
            FMOD.RESULT result;

            /*
                Create a System object and initialize.
            */
            result = FMOD.Factory.System_Create(ref system);
            ERRCHECK(result);

            result = system.getVersion(ref version);
            ERRCHECK(result);
            if (version < FMOD.VERSION.number)
            {
                throw new InvalidOperationException("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
            }
            result = system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            ERRCHECK(result);

            /*
                Bump up the file buffer size a little bit for netstreams (to account for lag).  Decode buffer is left at default.
            */
            result = system.setStreamBufferSize(64 * 1024, FMOD.TIMEUNIT.RAWBYTES);
            ERRCHECK(result);

            timer = new System.Timers.Timer();
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 10;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FMOD.RESULT result;
            FMOD.OPENSTATE openstate = 0;
            uint percentbuffered = 0;
            bool starving = false;
            bool busy = false;

            if (soundcreated)
            {
                result = sound.getOpenState(ref openstate, ref percentbuffered, ref starving, ref busy);
                ERRCHECK(result);

                if (openstate == FMOD.OPENSTATE.READY && channel == null)
                {
                    result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
                    ERRCHECK(result);
                }
            }

            if (channel != null)
            {
                uint ms = 0;
                bool playing = false;
                bool paused = false;

                bool metadataupdated = false;

                for (; ; )
                {
                    FMOD.TAG tag = new FMOD.TAG();
                    if (sound.getTag(null, -1, ref tag) != FMOD.RESULT.OK)
                    {
                        break;
                    }
                    if (tag.datatype == FMOD.TAGDATATYPE.STRING)
                    {
                        switch (tag.name.ToLower())
                        {
                            case "title":
                                _title = Marshal.PtrToStringAnsi(tag.data);
                                metadataupdated = true;
                                break;
                            case "artist":
                                _artist = Marshal.PtrToStringAnsi(tag.data);
                                metadataupdated = true;
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (metadataupdated)
                    if (MetadataChanged != null)
                        MetadataChanged(this, new PlaybackMetaDataChangedEventArgs()
                        {
                            Artist = _artist,
                            Track = _title
                        });

                result = channel.getPaused(ref paused);
                ERRCHECK(result);
                result = channel.isPlaying(ref playing);
                ERRCHECK(result);
                result = channel.getPosition(ref ms, FMOD.TIMEUNIT.MS);
                ERRCHECK(result);

                //statusBar.Text = "Time " + (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + ":" + (ms / 10 % 100) + (openstate == FMOD.OPENSTATE.BUFFERING ? " Buffering..." : (openstate == FMOD.OPENSTATE.CONNECTING ? " Connecting..." : (paused ? " Paused       " : (playing ? " Playing      " : " Stopped      ")))) + "(" + percentbuffered + "%)" + (starving ? " STARVING" : "        ");
            }

            if (system != null)
            {
                system.update();
            }
        }

        private void ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                timer.Stop();
            }
        }

        public void Play(string url)
        {
            FMOD.RESULT result;

            if (!soundcreated)
            {
                result = system.createSound(url, (FMOD.MODE.HARDWARE | FMOD.MODE._2D | FMOD.MODE.CREATESTREAM | FMOD.MODE.NONBLOCKING), ref sound);
                ERRCHECK(result);

                timer.Start();

                soundcreated = true;
            }
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            FMOD.RESULT result;
            bool        paused = false;

            if (channel != null)
            {
                result = channel.getPaused(ref paused);
                ERRCHECK(result);
                result = channel.setPaused(!paused);
                ERRCHECK(result);
            }
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
