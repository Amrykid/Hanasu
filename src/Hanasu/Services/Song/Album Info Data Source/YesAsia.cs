using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Text.RegularExpressions;
using System.Net;

namespace Hanasu.Services.Song.Album_Info_Data_Source
{
    public class YesAsia : IAlbumInfoDataSource
    {
        public bool GetAlbumInfo(ref SongData song)
        {
            try
            {
                var url = "http://www.yesasia.com/us/search-music/" + string.Join("-", (song.Artist.ToLower() + " " + song.TrackTitle.ToLower()).Split(' ')).Replace("-", ">>>") + "/0-0-0-bpt.48_bt.48_q." + string.Join("+", (song.Artist.ToLower() + " " + song.TrackTitle.ToLower()).Split(' ')).Replace("-", ">>>") + "-en/list.html";
                var html = HtmlTextUtility.GetHtmlFromUrl(url);
                //<span class="cover"><img alt="BoA Vol. 6 - Hurricane Venus" width="72" src="http://i.yai.bz/Assets/68/344/s_p0013234468.jpg"></span>

                var sec = Regex.Match(html, "<dd class=\"description\">.+?</dd>", RegexOptions.Compiled | RegexOptions.Singleline);
                var m = Regex.Match(sec.Value, "<span class=\"cover\">.+?</span>", RegexOptions.Compiled | RegexOptions.Singleline);
                var urlm = Regex.Match(m.Value, "src=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled);
                var imgurl = urlm.Value.Substring(5).Trim('\"');
                var albumtxt = Regex.Replace(Regex.Replace(Hanasu.Core.HtmlTextUtility.Decode(Regex.Match(m.Value, "alt=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value.Substring(5).Trim('\"')),
                    @"\((Korea|Japan|Hong Kong|Taiwan)\WVersion\)", "", RegexOptions.Compiled | RegexOptions.Singleline),
                    @"\((Normal|Special)\WEdition\)", "", RegexOptions.Compiled | RegexOptions.Singleline);

                var artist = Regex.Replace(Regex.Match(
                    Regex.Match(sec.Value, "<span class=\"artist\">.+?</span>.+?</span>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                    "<a.+?>.+?</a>", RegexOptions.Compiled | RegexOptions.Singleline).Value, "<.+?>", "");


                if (song.TrackTitle.ToLower() == artist.ToLower())
                {
                    //rare occasion when the artist and tracks are mixed up.

                    song.TrackTitle = song.Artist;
                    song.Artist = artist;
                }

                if (!artist.ToLower().StartsWith(song.Artist.ToLower())) return false;

                song.AlbumCoverUri = new Uri(imgurl.Replace("s_","l_"));

                if (song.AlbumCoverUri != null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        song.AlbumCoverData = wc.DownloadData(song.AlbumCoverUri);
                    }
                }

                song.Album = song.Album == null ? albumtxt : song.Album; // If an album is already there, leave it. otherwise, grab the one we get from YesAsia.

                var buylink = "http://www.yesasia.com" + Regex.Match(Regex.Match(sec.Value, "<a class=\"title\" href=\".+?\">", RegexOptions.Singleline | RegexOptions.Compiled).Value, "href=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value.Substring(5).Trim('\"');
                song.BuyUri = new Uri(buylink);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string WebsiteName
        {
            get { return "YesAsia"; }
        }
    }
}
