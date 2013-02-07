using Hanasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hanasu.Tools.Song
{
    public class SongService
    {
        public static string CleanSongDataStr(string songdata)
        {
            if (songdata.Contains("~") && songdata.Contains(" - ") && songdata.IndexOf(" - ") < songdata.IndexOf("~"))
                songdata = songdata.Substring(0, songdata.IndexOf("~"));
            else if (songdata.Contains(" ~ ") && !songdata.Contains(" - "))
                songdata = songdata.Replace(" ~ ", " - ").TrimEnd('-');

            songdata = Regex.Replace(songdata, @"\(.+?(\)|$)", "");

            songdata = songdata.Trim('\n', ' ');
            songdata = songdata.Replace("�", "");

            var tmpSplt = songdata.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

            if (tmpSplt.Length == 2)
            {
                if (Regex.IsMatch(songdata, NearFrontFeatureRegex))
                    songdata = Regex.Replace(songdata, NearFrontFeatureRegex, " -");
                else if (Regex.IsMatch(songdata, NearEndFeatureRegex))
                    songdata = Regex.Replace(songdata, NearEndFeatureRegex, "");
            }
            else if (tmpSplt.Length > 2)
            {
            }

            return songdata;
        }
        public const string NearFrontFeatureRegex = @"\W(ft|FT|feat|FEAT|Feat)\.(\W)?.+?\-";
        public const string NearEndFeatureRegex = @"\W(ft|FT|feat|FEAT|Feat)\..+?(\n|$)";

        public static async Task<bool> IsSongTitle(string name, Station currentStation)
        {
            //cheap way to check if its a song title. not perfect and doesn't work 100% of the time.

            return await Task.Run(() =>
                {
                    bool res = false;

                    if (name.Contains(" - "))
                    {
                        res = name.ToLower().Contains(currentStation.Title.ToLower()) == false && name.Split(' ').Length > 1;
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(name.ToLower(), @"(^)?(http\://)?[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?($|\W)?"))
                        res = false;

                    return res;
                });
        }

        static internal SongData ParseSongData(string songData, Station station)
        {
            var a = new SongData();

            songData = CleanSongDataStr(songData);
            var bits = songData.Split(new string[] { " - " }, 2, StringSplitOptions.None);

            a.Artist = bits[0];
            a.TrackTitle = bits[1];

            return a;
        }
    }
}
