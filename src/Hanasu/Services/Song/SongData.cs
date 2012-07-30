using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Services.Stations;

namespace Hanasu.Services.Song
{
    [Serializable]
    public struct SongData
    {
        [NonSerialized]
        public DateTime _timeStart;

        public string TrackTitle { get; set; }
        public string Artist { get; set; }
        public string Lyrics { get; set; }
        public Uri LyricsUri { get; set; }
        public string Album { get; set; }
        public Uri AlbumCoverUri { get; set; }
        public byte[] AlbumCoverData { get; set; }
        public Uri BuyUri { get; set; }
        public Station OriginallyPlayedStation { get; set; }
        public string OriginallyBroadcastSongData { get; set; }
        public TimeSpan EstimatedSongLength { get; set; }

        public override int GetHashCode()
        {
            if (TrackTitle != null && Artist != null)
                return TrackTitle.GetHashCode() ^ Artist.GetHashCode();
            else
                return base.GetHashCode();
        }

        public string ToSongString()
        {
            return Artist + " - " + TrackTitle;
        }
    }
}
