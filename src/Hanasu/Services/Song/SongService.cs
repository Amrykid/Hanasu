using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Hanasu.Services.Song.Lyric_Data_Sources;
using System.Collections.ObjectModel;
using Hanasu.Services.Song.Album_Info_Data_Source;
using System.Collections;
using Hanasu.Services.Stations;

namespace Hanasu.Services.Song
{
    public class SongService : IStaticService
    {

        private static Hashtable SongCache = null;

        static SongService()
        {
            SongCache = new Hashtable();
            DataSource = new YesAsia();
        }

        public static bool IsSongAvailable(string songdata, Station station, out Uri lyricsUri)
        {
            Hanasu.Services.Logging.LogService.Instance.WriteLog(typeof(SongService), "Checking if song is available from data: " + songdata);

            var newsongdata = CleanSongDataStr(songdata);

            Hanasu.Services.Logging.LogService.Instance.WriteLog(typeof(SongService), "Parsed song data into: " + newsongdata);

            if (SongCache.ContainsKey(newsongdata.ToLower()))
            {
                lyricsUri = ((SongData)SongCache[newsongdata.ToLower()]).LyricsUri; ;

                return true;
            }

            var datasource = new LetrasTerraLyricDataSource();

            var bits = newsongdata.Split(new string[] { " - " }, StringSplitOptions.None);


            string lyrics = null;

            try
            {

                if (datasource.GetLyrics(bits[0].Trim(' ', ' '), bits[1].Trim(' ', ' '), out lyrics, out lyricsUri))
                {
                    var song = new SongData();

                    song.Artist = bits[0].Trim(' ');
                    song.TrackTitle = bits[1].Trim(' ');
                    song.Lyrics = lyrics;
                    song.LyricsUri = lyricsUri;
                    song.OriginallyPlayedStation = station;
                    song.OriginallyBroadcastSongData = songdata;

                    DataSource.GetAlbumInfo(ref song);

                    SongCache.Add(newsongdata.ToLower(), song);

                    return true;
                }
                else if (datasource.GetLyrics(bits[1].Trim(' ', ' '), bits[0].Trim(' ', ' '), out lyrics, out lyricsUri))
                {
                    var song = new SongData();

                    song.Artist = bits[1].Trim(' ');
                    song.TrackTitle = bits[0].Trim(' ');
                    song.Lyrics = lyrics;
                    song.LyricsUri = lyricsUri;
                    song.OriginallyPlayedStation = station;
                    song.OriginallyBroadcastSongData = songdata;

                    DataSource.GetAlbumInfo(ref song);

                    SongCache.Add(newsongdata.ToLower(), song);

                    return true;
                }
            }
            catch (Exception)
            {
            }
            lyricsUri = null;

            return false;
        }
        public static SongData GetSongData(string songdata, Station station)
        {
            if (SongCache.ContainsKey(CleanSongDataStr(songdata).ToLower()))
                return (SongData)SongCache[CleanSongDataStr(songdata).ToLower()];

            Uri lyricsUrl = null;
            if (IsSongAvailable(songdata, station, out lyricsUrl))
                return (SongData)SongCache[CleanSongDataStr(songdata).ToLower()];

            throw new Exception("Song data doesn't exist");
        }
        public static string CleanSongDataStr(string songdata)
        {
            if (songdata.Contains("~") && songdata.Contains(" - ") && songdata.IndexOf(" - ") < songdata.IndexOf("~"))
                songdata = songdata.Substring(0, songdata.IndexOf("~"));
            else if (songdata.Contains(" ~ ") && !songdata.Contains(" - "))
                songdata = songdata.Replace(" ~ ", " - ").TrimEnd('-');

            songdata = Regex.Replace(songdata, @"\(.+?(\)|$)", "", RegexOptions.Compiled);

            songdata = songdata.Trim('\n', ' ');
            songdata = songdata.Replace("�", "");

            var tmpSplt = songdata.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

            if (tmpSplt.Length == 2)
            {
                if (Regex.IsMatch(songdata, NearEndFeatureRegex, RegexOptions.Compiled))
                    songdata = Regex.Replace(songdata, NearEndFeatureRegex, "", RegexOptions.Compiled);
                else if (Regex.IsMatch(songdata, NearFrontFeatureRegex, RegexOptions.Compiled))
                    songdata = Regex.Replace(songdata, NearFrontFeatureRegex, " -", RegexOptions.Compiled);
            }
            else if (tmpSplt.Length > 2)
            {
            }

            return songdata;
        }
        public const string NearFrontFeatureRegex = @"\W(ft|FT|feat|FEAT)\.(\W)?.+?\-";
        public const string NearEndFeatureRegex = @"\W(ft|FT|feat|FEAT)\..+?(\n|$)";

        public static bool IsSongTitle(string name, Station currentStation)
        {
            //cheap way to check if its a song title. not perfect and doesn't work 100% of the time.

#if DEBUG
            System.Diagnostics.Debug.WriteLine("IsSongTitle: " + name + " |||| " + currentStation.Name);
#endif

                if (name.Contains(" - "))
                {
                    return name.ToLower().Contains(currentStation.Name.ToLower()) == false && name.Split(' ').Length > 1;
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(name.ToLower(), @"(^)?(http\://)?[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?($|\W)?", System.Text.RegularExpressions.RegexOptions.Compiled))
                    return false;
                else
                    return Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name);
        }

        public static IAlbumInfoDataSource DataSource { get; set; }
    }
}
