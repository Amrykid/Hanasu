using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Hanasu.Core.Songs.Lyric_Data_Sources;
using System.Collections.ObjectModel;
using Hanasu.Core.Songs.Album_Info_Data_Source;
using System.Collections;
using Hanasu.Core.Stations;
using System.Net;

namespace Hanasu.Core.Songs
{
    public class SongService
    {
        private Hashtable SongCache = null;

        internal SongService()
        {
            SongCache = new Hashtable();
            DataSource = new YesAsia();
            LyricsSource = new TTPlayerLyricsDataSource();
        }

        public bool IsSongAvailable(string songdata, Station station, out Uri lyricsUri)
        {

            var newsongdata = CleanSongDataStr(songdata);

            if (SongCache.ContainsKey(newsongdata.ToLower()))
            {
                lyricsUri = ((SongData)SongCache[newsongdata.ToLower()]).LyricsUri; ;

                return true;
            }

            ILyricsDataSource lyricssource = null;
            if (station.PreferredLyricsSource == null)
                lyricssource = LyricsSource;
            else
                lyricssource = AllLyricsSources.First(t => t.WebsiteName == station.PreferredLyricsSource);


            var bits = newsongdata.Split(new string[] { " - " }, StringSplitOptions.None);


            object lyrics = null;

            try
            {
                bool issynchronized;
                if (lyricssource.GetLyrics(bits[0].Trim(' ', ' '), bits[1].Trim(' ', ' '), out lyrics, out lyricsUri, out issynchronized))
                {
                    var song = new SongData();

                    song.Artist = bits[0].Trim(' ');
                    song.TrackTitle = bits[1].Trim(' ');
                    song.Lyrics = lyrics;
                    song.LyricsUri = lyricsUri;
                    song.LyricsSynchronized = issynchronized;
                    song.OriginallyPlayedStation = station;
                    song.OriginallyBroadcastSongData = songdata;

                    if (station.PreferredStoreSource == null)
                        DataSource.GetAlbumInfo(ref song);
                    else
                        AllDataSources.First(t => t.WebsiteName == station.PreferredStoreSource).GetAlbumInfo(ref song);

                    SongCache.Add(newsongdata.ToLower(), song);

                    return true;
                }
                else if (lyricssource.GetLyrics(bits[1].Trim(' ', ' '), bits[0].Trim(' ', ' '), out lyrics, out lyricsUri, out issynchronized))
                {
                    var song = new SongData();

                    song.Artist = bits[1].Trim(' ');
                    song.TrackTitle = bits[0].Trim(' ');
                    song.Lyrics = lyrics;
                    song.LyricsUri = lyricsUri;
                    song.LyricsSynchronized = issynchronized;
                    song.OriginallyPlayedStation = station;
                    song.OriginallyBroadcastSongData = songdata;

                    if (station.PreferredStoreSource == null)
                        DataSource.GetAlbumInfo(ref song);
                    else
                        AllDataSources.First(t => t.WebsiteName == station.PreferredStoreSource).GetAlbumInfo(ref song);

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
        public SongData GetSongData(string songdata, Station station)
        {
            if (SongCache.ContainsKey(CleanSongDataStr(songdata).ToLower()))
                return (SongData)SongCache[CleanSongDataStr(songdata).ToLower()];

            Uri lyricsUrl = null;
            if (IsSongAvailable(songdata, station, out lyricsUrl))
                return (SongData)SongCache[CleanSongDataStr(songdata).ToLower()];

            throw new Exception("Song data doesn't exist");
        }
        public string CleanSongDataStr(string songdata)
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
                if (Regex.IsMatch(songdata, NearFrontFeatureRegex, RegexOptions.Compiled))
                    songdata = Regex.Replace(songdata, NearFrontFeatureRegex, " -", RegexOptions.Compiled);
                else if (Regex.IsMatch(songdata, NearEndFeatureRegex, RegexOptions.Compiled))
                    songdata = Regex.Replace(songdata, NearEndFeatureRegex, "", RegexOptions.Compiled);
            }
            else if (tmpSplt.Length > 2)
            {
            }

            return songdata;
        }
        public const string NearFrontFeatureRegex = @"\W(ft|FT|feat|FEAT|Feat)\.(\W)?.+?\-";
        public const string NearEndFeatureRegex = @"\W(ft|FT|feat|FEAT|Feat)\..+?(\n|$)";

        public bool IsSongTitle(string name, Station currentStation)
        {
            //cheap way to check if its a song title. not perfect and doesn't work 100% of the time.

#if DEBUG
            System.Diagnostics.Debug.WriteLine("IsSongTitle: " + name + " |||| " + currentStation.Name);
#endif
            bool res = false;

            if (name.Contains(" - "))
            {
                res = name.ToLower().Contains(currentStation.Name.ToLower()) == false && name.Split(' ').Length > 1;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(name.ToLower(), @"(^)?(http\://)?[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?($|\W)?", System.Text.RegularExpressions.RegexOptions.Compiled))
                res = false;
            //else
            //    res = Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name);

            return res;
        }

        public IAlbumInfoDataSource DataSource { get; set; }
        public IAlbumInfoDataSource[] AllDataSources { get { return new IAlbumInfoDataSource[] { DataSource, new MSMetaServices() }; } }
        public ILyricsDataSource LyricsSource { get; set; }
        public ILyricsDataSource[] AllLyricsSources { get { return new ILyricsDataSource[] { new LetrasTerraLyricDataSource(), LyricsSource }; } }

        internal SongData ParseSongData(string songData, Station station)
        {
            var a = new SongData();

            songData = CleanSongDataStr(songData);
            var bits = songData.Split(new string[] { " - " }, 2, StringSplitOptions.None);

            a.Artist = bits[0];
            a.TrackTitle = bits[1];

            return a;
        }

        internal bool GetLyricsFromSong(ref SongData x)
        {
            object lyrics = null;
            Uri lyricsUri = null;

            try
            {
                bool issynchronized;

                LyricsSource.GetLyrics(x.Artist, x.TrackTitle, out lyrics, out lyricsUri, out issynchronized);

                x.Lyrics = lyrics;
                x.LyricsSynchronized = issynchronized;
                x.LyricsUri = lyricsUri;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
