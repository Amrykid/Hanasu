using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Songs;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;

namespace Hanasu.Core.Stations.Shoutcast
{
    public static class ShoutcastService
    {
        private static List<string> cached_Shoutcasturls = new List<string>();
        public static bool GetIfShoutcastStation(Hashtable WMPplayerAttributes)
        {
            if (WMPplayerAttributes.ContainsKey("SourceURL"))
            {
                var url = (string)WMPplayerAttributes["SourceURL"];

                if (cached_Shoutcasturls.Contains(url)) return true;

                if (!url.StartsWith("http") && !url.StartsWith("https")) return false;

                return GetIfShoutcastStation(url);
            }
            return false;
        }

        public static bool GetIfShoutcastStation(string url)
        {
            try
            {
                var html = Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(url);

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

        public static Dictionary<string, string> GetShoutcastStationSongHistory(Station station, Hashtable playerAttributes)
        {
            var url = (string)playerAttributes["SourceURL"];

            return GetShoutcastStationSongHistory(station, url);
        }
        public static Dictionary<string, string> GetShoutcastStationSongHistory(Station station, string url)
        {
            var his = new Dictionary<string, string>();

            if (url.EndsWith("/") == false)
                url += "/";
            url += "played.html";

            var html = Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(url);

            var songtable = Regex.Matches(html, "<table.+?>.+?</table>", RegexOptions.Singleline | RegexOptions.Compiled)[1];
            var entries = Regex.Matches(songtable.Value,
                "<tr>.+?</tr>",
                RegexOptions.Singleline | RegexOptions.Compiled);

            for (int i = 1; i < entries.Count; i++)
            {
                var entry = entries[i];
                var bits = Regex.Matches(
                        Regex.Replace(
                            entry.Value, "<b>Current Song</b>", "", RegexOptions.Compiled | RegexOptions.Singleline),
                    "<td>.+?(</td>|</tr>)", RegexOptions.Compiled | RegexOptions.Singleline);

                var key = Regex.Replace(bits[0].Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled).Trim();
                var val = Regex.Replace(bits[1].Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled).Trim();
                if (his.ContainsKey(key) == false)
                    his.Add(key,
                        val);
            }


            return his;
        }
        public static DateTime GetShoutcastStationCurrentSongStartTime(Station station, Hashtable playerAttributes)
        {
            var things = GetShoutcastStationSongHistory(station, playerAttributes);
            var lastime = things.Keys.First();

            var stationTZ = station.TimeZoneInfo;
            var local = TimeZoneInfo.Local;

            var time = DateTime.Parse(lastime);

            var newtime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                time,
                stationTZ.Id,
                local.Id);

            return newtime;
        }
        public static DateTime GetShoutcastStationCurrentSongStartTime(Station station, string url)
        {

            var things = GetShoutcastStationSongHistory(station, url);
            var lastime = things.Keys.First();

            var stationTZ = station.TimeZoneInfo;
            var local = TimeZoneInfo.Local;

            var time = DateTime.Parse(lastime);

            var newtime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                time,
                stationTZ.Id,
                local.Id);

            return newtime;
        }
        public static SongData GetShoutcastStationCurrentSong(Station station, Hashtable playerAttributes)
        {
            var url = (string)playerAttributes["SourceURL"];
            return GetShoutcastStationCurrentSong(station, url);
        }
        public static SongData GetShoutcastStationCurrentSong(Station station, string url)
        {

            var html = Hanasu.Core.Utilities.HtmlTextUtility.GetHtmlFromUrl2(url.ToString());
            var songtable = Regex.Matches(html, "<table.+?>.+?</table>", RegexOptions.Singleline | RegexOptions.Compiled)[2];
            var entries = Regex.Matches(songtable.Value,
                "<tr>.+?</tr>",
                RegexOptions.Singleline | RegexOptions.Compiled);

            var songEntry = entries[entries.Count - 1];
            var songData = Regex.Match(songEntry.Value, "<b>.+?</b>", RegexOptions.Singleline | RegexOptions.Compiled).Value;
            songData = Regex.Replace(songData, "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline).Trim();
            songData = GlobalHanasuCore.SongService.CleanSongDataStr(songData);

            Uri lyrics = null;
            if (GlobalHanasuCore.SongService.IsSongAvailable(songData, station, out lyrics))
                return GlobalHanasuCore.SongService.GetSongData(songData, station);
            else
                if (GlobalHanasuCore.SongService.IsSongTitle(songData, station))
                    return GlobalHanasuCore.SongService.ParseSongData(songData, station);
                else throw new Exception();
        }
    }
}
