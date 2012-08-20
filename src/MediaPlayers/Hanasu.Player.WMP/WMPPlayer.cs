using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;
using System.ComponentModel.Composition;
using Hanasu.Core;
using WMPLib;
using System.Collections;
using System.Windows.Forms.Integration;

namespace Hanasu.Player.WMP
{
    [Export(typeof(IMediaPlayer))]
    public class WMPPlayer : IMediaPlayer
    {
        AxWMP host = null;
        AxWMPLib.AxWindowsMediaPlayer player = null;

        private Hashtable currentStationAttributes = new Hashtable();

        private string songName = null;

        private WindowsFormsHost wpfhost = null;

        public void Initialize()
        {
            host = new AxWMP();
            player = host.axWindowsMediaPlayer1;
            player.settings.autoStart = false;
            player.MediaChange += player_MediaChange;
            player.MediaError += player_MediaError;
            player.PlayStateChange += player_PlayStateChange;
            player.uiMode = "none";
            player.stretchToFit = true;
            //player.Dock = System.Windows.Forms.DockStyle.Fill;
            player.enableContextMenu = false;
            player.SendToBack();
            host.SendToBack();

            wpfhost = new WindowsFormsHost()
            {
                Child = host,
            };
        }

        void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            switch ((WMPLib.WMPPlayState)e.newState)
            {
                case WMPPlayState.wmppsPlaying:
                    {
                        parseAttributes();

                        GlobalHanasuCore.OnStationMediaTypeDetected(this, IsVideo);
                    }
                    break;
            }

        }

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
        {
            GlobalHanasuCore.OnStationConnectionTerminated(this);

            songName = null;
        }

        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            //Differentating Song titles, station titles and radio messages is basic for now. Will be expanded to be equal to or better than Hanasu 1.0. It also differs based on the player selected.

            var name = player.currentMedia.name;

            if (name.StartsWith(GlobalHanasuCore.CurrentStation.Name))
                GlobalHanasuCore.OnStationTitleDetected(this, name);
            else
                if (name.Contains(" - ") && songName != name)
                {
                    GlobalHanasuCore.OnSongTitleDetected(this, name);
                    songName = name;
                }
                else
                {
                    //Radio message
                }

        }

        private void parseAttributes()
        {
            currentStationAttributes.Clear();
            for (int i = 0; i < player.currentMedia.attributeCount; i++)
            {
                var x = player.currentMedia.getAttributeName(i);
                var y = player.currentMedia.getItemInfo(x);

                currentStationAttributes.Add(x, y);
            }
        }

        public void Play(Uri url)
        {
            player.URL = url.ToString();
            player.Ctlcontrols.play();
        }

        public void Stop()
        {
            player.Ctlcontrols.stop();

            songName = null;
        }


        public bool IsPlaying
        {
            get { return player.playState == WMPLib.WMPPlayState.wmppsPlaying || player.playState == WMPLib.WMPPlayState.wmppsBuffering; }
        }


        public void Shutdown()
        {
            if (player != null)
            {
                player.MediaChange -= player_MediaChange;
                player.MediaError -= player_MediaError;
                player.PlayStateChange -= player_PlayStateChange;
                //player.close();

                try
                {
                    player.Dispose();
                    host.Dispose();

                    player = null;
                    host = null;
                }
                catch (Exception)
                {
                }
            }
        }
        ~WMPPlayer()
        {
            Shutdown();
        }


        public bool Supports(string extension)
        {
            var supported = new string[] { ".asx", ".mp3", ".wav" }; //WMP supports M3U but I would like Hanasu to give people the choice to choose a stream.

            return supported.Contains(extension);
        }


        public int Volume
        {
            get
            {
                if (player != null)
                    return player.settings.volume;

                return 0;
            }
            set
            {
                if (player != null)
                    player.settings.volume = value;
            }
        }


        public bool IsVideo
        {
            get
            {
                if (currentStationAttributes.ContainsKey("MediaType"))
                    return currentStationAttributes["MediaType"].ToString().ToLower() == "video" || currentStationAttributes["MediaType"].ToString().ToLower() == "photo";

                return false;
            }
        }

        public object GetVideoControl()
        {
            return wpfhost;
        }

        public bool IsWMP12OrHigher()
        {
            if (player != null)
            {
                var x = Version.Parse(player.versionInfo);

                return x.Major >= 12;
            }
            else
            {
                return Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major > 6 && Environment.OSVersion.Version.Minor >= 1); //Educated guess because Vista (6.0 can only have WMP 11) and 7 (6.1) can have WMP12.
            }

            return false;
        }
    }
}
