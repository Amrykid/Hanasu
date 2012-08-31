using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Core.Songs
{
    public interface ILyricsDataSource
    {
        bool GetLyrics(string artist, string track, out object lyrics, out Uri lyricsUri, out bool isSynchronizedLyrics);
        string WebsiteName { get; }
        string LyricFormatUrl { get; }
    }
}
