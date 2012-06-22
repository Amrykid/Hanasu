using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;

namespace Hanasu.Services.Song.Album_Info_Data_Source
{
    public class YesAsia : IAlbumInfoDataSource
    {
        public bool GetAlbumInfo(ref SongData song)
        {
            try
            {
                var url = "http://www.yesasia.com/us/search/" + string.Join("-", (song.Artist.ToLower() + " " + song.TrackTitle.ToLower()).Split(' ')).Replace("-", ">>>") + "/0-0-0-bpt.48_q." + string.Join("+", (song.Artist.ToLower() + " " + song.TrackTitle.ToLower()).Split(' ')).Replace("-", ">>>") + "-en/list.html";
                var html = HtmlDecoder.GetHTML(url);
                //<span class="cover"><img alt="BoA Vol. 6 - Hurricane Venus" width="72" src="http://i.yai.bz/Assets/68/344/s_p0013234468.jpg"></span>

                var m = System.Text.RegularExpressions.Regex.Match(html, "<span class=\"cover\">.+?</span>", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Singleline);
                var urlm = System.Text.RegularExpressions.Regex.Match(m.Value, "src=\".+?\"", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Compiled);
                var imgurl = urlm.Value.Substring(5).Trim('\"');

                song.AlbumCoverUri = new Uri(imgurl);

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
