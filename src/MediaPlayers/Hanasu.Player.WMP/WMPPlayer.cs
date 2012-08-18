using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;
using System.ComponentModel.Composition;
using Hanasu.Core;

namespace Hanasu.Player.WMP
{
    [Export(typeof(IMediaPlayer))]
    public class WMPPlayer : IMediaPlayer
    {
        AxWMP host = null;
        AxWMPLib.AxWindowsMediaPlayer player = null;

        public void Initialize()
        {
            host = new AxWMP();
            player = host.axWindowsMediaPlayer1;
            player.settings.autoStart = false;
            player.MediaChange += player_MediaChange;
            player.MediaError += player_MediaError;
        }

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
        {
            GlobalHanasuCore.OnStationConnectionTerminated(this);
        }

        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            //Differentating Song titles, station titles and radio messages is basic for now. Will be expanded to be equal to or better than Hanasu 1.0. It also differs based on the player selected.

            var name = player.currentMedia.name;

            if (name.StartsWith(GlobalHanasuCore.CurrentStation.Name))
                GlobalHanasuCore.OnStationTitleDetected(this, name);
            else
                if (name.Contains(" - "))
                {
                    GlobalHanasuCore.OnSongTitleDetected(this, name);
                }
                else
                {
                    //Radio message
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
    }
}
