using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Song
{
    public class SongData
    {
        public string TrackTitle { get; set; }
        public string Artist { get; set; }
        public string Lyrics { get; set; }
        public Uri LyricsUri { get; set; }
        public string Album { get; set; }
        public Uri AlbumCoverUri { get; set; }
        public Uri BuyUri { get; set; }
    }
}
