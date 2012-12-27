using Hanasu.Core.Utilities;
using Hanasu.Model;
using Hanasu.Tools.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hanasu.Tools.Shoutcast
{
    public static class ShoutcastService
    {
        private static List<string> cached_Shoutcasturls = new List<string>();

        public static async Task<bool> GetIfShoutcastStation(string url)
        {
            try
            {
                var html = await Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(url);

                var res = html.Contains("SHOUTcast D.N.A.S. Status</font>");

                if (res)
                    cached_Shoutcasturls.Add(url);

                return res;
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static async Task<Dictionary<string, string>> GetShoutcastStationSongHistory(Station station, string url)
        {
           
            if (url.EndsWith("/") == false)
                url += "/";
            url += "played.html";

            var html = await Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(url);

            var songtable = Regex.Matches(html, "<table.+?>.+?</table>", RegexOptions.Singleline)[1];
            var entries = Regex.Matches(songtable.Value,
                "<tr>.+?</tr>",
                RegexOptions.Singleline);

            var his = new Dictionary<string, string>();

            await Task.Run(() =>
                {
                    for (int i = 1; i < entries.Count; i++)
                    {
                        var entry = entries[i];
                        var bits = Regex.Matches(
                                Regex.Replace(
                                    entry.Value, "<b>Current Song</b>", "", RegexOptions.Singleline),
                            "<td>.+?(</td>|</tr>)", RegexOptions.Singleline);

                        var key = Regex.Replace(bits[0].Value, "<.+?>", "", RegexOptions.Singleline).Trim();
                        var val = Regex.Replace(bits[1].Value, "<.+?>", "", RegexOptions.Singleline).Trim();
                        if (his.ContainsKey(key) == false)
                            his.Add(key,
                                val);
                    }
                });

            return his;
        }
        public static async Task<SongData> GetShoutcastStationCurrentSong(Station station, string url)
        {

            var html = await HtmlTextUtility.GetHtmlFromUrl2(url.ToString().Replace("\r",""));
            var songtable = Regex.Matches(html, "<table.+?>.+?</table>", RegexOptions.Singleline)[2];
            var entries = Regex.Matches(songtable.Value,
                "<tr>.+?</tr>",
                RegexOptions.Singleline);

            var songEntry = entries[entries.Count - 1];
            var songData = Regex.Match(songEntry.Value, "<b>.+?</b>", RegexOptions.Singleline).Value;
            songData = Regex.Replace(songData, "<.+?>", "", RegexOptions.Singleline).Trim();
            songData = SongService.CleanSongDataStr(songData);

            Uri lyrics = null;
            //if (GlobalHanasuCore.SongService.IsSongAvailable(songData, station, out lyrics))
            //    return GlobalHanasuCore.SongService.GetSongData(songData, station);
            //else
            if (await SongService.IsSongTitle(songData, station))
                return SongService.ParseSongData(songData, station);
            else throw new Exception();
        }
    }
}
