using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Hanasu.Core;

namespace Hanasu.Services.Song.Lyric_Data_Sources
{
    class LetrasTerraLyricDataSource : ILyricsDataSource
    {
        public bool GetLyrics(string artist, string track, out string lyrics, out Uri lyricsUri)
        {
            Hanasu.Services.Logging.LogService.Instance.WriteLog(this, "Attempting to find lyrics for artist: " + artist + " - track: " + track);

            var url = String.Format(LyricFormatUrl,
                        HtmlTextUtility.UrlEncode(artist),
                        HtmlTextUtility.UrlEncode(track));

            using (WebClient wc = new WebClient())
            {
                var data = wc.DownloadString(
                    url);

                if (!data.Contains("<div id=\"cabecalho\">")) //Bootlegged way to check if it was found
                {
                    lyrics = null;

                    lyricsUri = null;

                    return false; //Not found.
                }
                else
                {

                    var ptags = Regex.Matches(data, "<p>.+?</p>", RegexOptions.Singleline | RegexOptions.Compiled);
                    var lyricdata = ptags[ptags.Count - 1].Value;

                    lyricdata = Regex.Replace(lyricdata, "<br/><br/>", "\r\n\r\n", RegexOptions.Compiled);

                    lyricdata = Regex.Replace(lyricdata, "<.+?>", "", RegexOptions.Compiled);

                    lyrics = HtmlTextUtility.Decode(lyricdata);

                    if (lyrics == null)
                    {
                        lyricsUri = null;
                        return false; //Rare cause when my bootlegged way doesn't work
                    }
                    else
                    {
                        lyricsUri = new Uri(url);

                        return true;
                    }
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
