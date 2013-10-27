using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanasuWP8
{
    public static class BackgroundAudioPlayerExtensions
    {
        public static PlayState SafeGetPlayerState(this BackgroundAudioPlayer player)
        {
            try
            {
                return player.PlayerState;
            }
            catch (Exception)
            {
                return PlayState.Unknown;
            }
        }
    }
}
