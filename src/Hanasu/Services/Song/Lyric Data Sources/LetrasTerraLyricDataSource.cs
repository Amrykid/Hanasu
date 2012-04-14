using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace Hanasu.Services.Song.Lyric_Data_Sources
{
    class LetrasTerraLyricDataSource : ILyricsDataSource
    {
        public bool GetLyrics(string artist, string track, out string lyrics, out Uri lyricsUri)
        {
            var url = String.Format(LyricFormatUrl,
                        artist,
                        track);

            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(
                    url);

                if (data.Contains("MÃºsica nÃ£o encontrada")) //Bootlegged way to check if it was found
                {
                    lyrics = null;

                    lyricsUri = null;

                    return false; //Not found.
                }
                else
                {

                    var lyricdata = Regex.Match(data, "<p>.+?</p>",RegexOptions.Singleline).Value;

                    lyricdata = Regex.Replace(lyricdata, "<br/><br/>", "\r\n\r\n");

                    lyricdata = Regex.Replace(lyricdata, "<.+?>", "");

                    lyrics = lyricdata;

                    lyricsUri = new Uri(url);

                    return true;
                }
            }
        }

        public string WebsiteName
        {
            get { return "Letras Terra"; }
        }

        public string LyricFormatUrl
        {
            get { return "http://letras.terra.com.br/winamp.php?musica={1}&artista={0}"; }
        }
    }
}
