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
    public class WMPPlayer: IMediaPlayer
    {
        AxWMP host = null;
        AxWMPLib.AxWindowsMediaPlayer player = null;

        public void Initialize()
        {
            host = new AxWMP();
            player = host.axWindowsMediaPlayer1;
            player.settings.autoStart = false;
            player.MediaChange += new AxWMPLib._WMPOCXEvents_MediaChangeEventHandler(player_MediaChange);
        }

        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            var name = player.currentMedia.name;

            
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
    }
}
