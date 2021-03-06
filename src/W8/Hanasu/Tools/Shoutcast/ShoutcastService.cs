﻿using Hanasu.Core.Utilities;
using Hanasu.Model;
#if !WINDOWS_PHONE
using Hanasu.Tools.Song;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            //return false;
        }

        public static async Task<ObservableCollection<ShoutcastSongHistoryItem>> GetShoutcastStationSongHistory(Station station, string url)
        {
            var items = await GetShoutcastStationSongHistoryOld(station, url);

            var coll = new ObservableCollection<ShoutcastSongHistoryItem>();

            foreach (var item in items)
                coll.Add(new ShoutcastSongHistoryItem() { Time = DateTime.Parse(item.Key), Song = item.Value });


            return coll;
        }
        public static async Task<Dictionary<string, string>> GetShoutcastStationSongHistoryOld(Station station, string url)
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
#if !WINDOWS_PHONE
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

            //Uri lyrics = null;
            //if (GlobalHanasuCore.SongService.IsSongAvailable(songData, station, out lyrics))
            //    return GlobalHanasuCore.SongService.GetSongData(songData, station);
            //else
            if (await SongService.IsSongTitle(songData, station))
                return SongService.ParseSongData(songData, station);
            else throw new Exception();
        }

        public static async Task<ShoutcastStream> GetShoutcastStream(Uri url)
        {
            var s = new ShoutcastStream();
            await s.ConnectAsync(url);

            return s;
        }
#endif
    }

    public struct ShoutcastSongHistoryItem
    {
        public DateTime Time { get; set; }
        public string Song { get; set; }

        public string LocalizedTime { get; set; }
    }
}
