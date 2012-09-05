using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace Hanasu.Core.Songs.Lyric_Data_Sources
{
    public class TTPlayerLyricsDataSource : ILyricsDataSource
    {
        private Hashtable cachedLyrics = new Hashtable();

        public bool GetLyrics(string artist, string track, out object lyrics, out Uri lyricsUri, out bool isSynchronizedLyrics)
        {
            Lyrics result = null;
            var tuple = new Tuple<string, string>(artist, track);
            if (cachedLyrics.ContainsKey(tuple))
                result = (Lyrics)cachedLyrics[tuple];
            else
            {
                result = LyricsAssistant.GetLyrics(artist, track);
                cachedLyrics.Add(tuple, result);
            }

            if (cachedLyrics.Count > 20)
            {
                //implement some sort of memory saving, collection cleaning crap.
            }

            lyricsUri = null;
            isSynchronizedLyrics = true;

            if (result == null) lyrics = null;
            else lyrics = result.TimeAndLyrics;

            return result != null;
        }

        public string WebsiteName
        {
            get { return "TTPlayer lyrics site."; }
        }

        public string LyricFormatUrl
        {
            get { return null; }
        }
    }

    //Every below has been modified from DoubanFM: http://doubanfm.codeplex.com
    //All credit goes to him for his conversion of: http://www.iscripts.org/forum.php?mod=viewthread&action=printable&tid=85
    public static class LyricsAssistant
    {
        /// <summary>
        /// 歌词服务器列表
        /// </summary>
        private static readonly string[] servers = new string[] { "ttlrcct.qianqian.com", "ttlrcct2.qianqian.com" };

        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="artist">表演者</param>
        /// <param name="title">标题</param>
        public static Lyrics GetLyrics(string artist, string title)
        {
            if (string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(title)) return null;
            if (title.ToLower().Contains("instrumental")) return null;

            //获取所有可能的歌词


            foreach (var server in servers)
            {
                using (WebClient wc = new WebClient())
                {
                    string url = String.Format("http://" + server + "/dll/lyricsvr.dll?sh?Artist={0}&Title={1}&Flag=0", Encode(artist), Encode(title));
                    string file = wc.DownloadString(url);

                    //分析返回的XML文件
                    LyricsResult result = null;
                    try
                    {
                        using (MemoryStream stream = new MemoryStream())
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(file);
                            writer.Flush();
                            XmlSerializer serializer = new XmlSerializer(typeof(LyricsResult));
                            stream.Position = 0;
                            result = (LyricsResult)serializer.Deserialize(stream);
                        }
                    }
                    catch { }
                    if (result == null || result.Count == 0) continue;

                    //选出最合适的歌词文件
                    LyricsItem selected = result[0];
                    //double dist = double.MaxValue;
                    string lArtist = artist.ToLower();
                    string lTitle = title.ToLower();


                    //下载歌词文件
                    string url2 = String.Format("http://" + server + "/dll/lyricsvr.dll?dl?Id={0}&Code={1}", selected.Id.ToString(), VerifyCode(selected.Artist, selected.Title, selected.Id));
                    string file2 = wc.DownloadString(url2);

                    //生成Lyrics的实例
                    if (string.IsNullOrEmpty(file2)) continue;
                    try
                    {
                        return new Lyrics(file2);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 对数据编码
        /// </summary>
        static string Encode(string data)
        {
            if (data == null) return "";
            string temp = data.Replace(" ", "").Replace("'", "").ToLower();
            byte[] bytes = Encoding.Unicode.GetBytes(temp);
            StringBuilder sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(Uri.HexEscape((char)b).Replace("%", ""));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 生成校验码
        /// </summary>
        static string VerifyCode(string artist, string title, int lrcId)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(artist + title);
            int[] song = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                song[i] = bytes[i] & 0xff;
            int intVal1 = 0, intVal2 = 0, intVal3 = 0;
            intVal1 = (lrcId & 0xFF00) >> 8;
            if ((lrcId & 0xFF0000) == 0)
            {
                intVal3 = 0xFF & ~intVal1;
            }
            else
            {
                intVal3 = 0xFF & ((lrcId & 0x00FF0000) >> 16);
            }
            intVal3 = intVal3 | ((0xFF & lrcId) << 8);
            intVal3 = intVal3 << 8;
            intVal3 = intVal3 | (0xFF & intVal1);
            intVal3 = intVal3 << 8;
            if ((lrcId & 0xFF000000) == 0)
            {
                intVal3 = intVal3 | (0xFF & (~lrcId));
            }
            else
            {
                intVal3 = intVal3 | (0xFF & (lrcId >> 24));
            }
            int uBound = bytes.Length - 1;
            while (uBound >= 0)
            {
                int c = song[uBound];
                if (c >= 0x80)
                    c = c - 0x100;
                intVal1 = c + intVal2;
                intVal2 = intVal2 << (uBound % 2 + 4);
                intVal2 = intVal1 + intVal2;
                uBound -= 1;
            }
            uBound = 0;
            intVal1 = 0;
            while (uBound <= bytes.Length - 1)
            {
                int c = song[uBound];
                if (c >= 128)
                    c = c - 256;
                int intVal4 = c + intVal1;
                intVal1 = intVal1 << (uBound % 2 + 3);
                intVal1 = intVal1 + intVal4;
                uBound += 1;
            }
            int intVal5 = intVal2 ^ intVal3;
            intVal5 = intVal5 + (intVal1 | lrcId);
            intVal5 = intVal5 * (intVal1 | intVal3);
            intVal5 = intVal5 * (intVal2 ^ lrcId);
            return intVal5.ToString();
        }
    }
    [XmlRootAttribute("result")]
    public class LyricsResult : List<LyricsItem>
    {
    }
    /// <summary>
    /// 表示歌词搜索结果中的一个歌词文件
    /// </summary>
    [XmlTypeAttribute("lrc")]
    public class LyricsItem
    {
        /// <summary>
        /// ID
        /// </summary>
        [XmlAttribute("id")]
        public int Id { get; set; }
        /// <summary>
        /// 艺术家
        /// </summary>
        [XmlAttribute("artist")]
        public string Artist { get; set; }
        /// <summary>
        /// 曲目名称
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }
    }
    /// <summary>
    /// 表示一个LRC格式的歌词
    /// </summary>
    public class Lyrics
    {
        #region values

        /// <summary>
        /// 获取LRC歌词代码
        /// </summary>
        public string LrcCode { get; private set; }
        /// <summary>
        /// 获取原始字典
        /// </summary>
        public Dictionary<string, string> Dictionary { get; private set; }
        /// <summary>
        /// 时间、歌词字典
        /// </summary>
        public Dictionary<TimeSpan, string> TimeAndLyrics { get; set; }
        /// <summary>
        /// 排过序的时间列表
        /// </summary>
        public List<TimeSpan> SortedTimes { get; private set; }
        /// <summary>
        /// 返回当前的歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string CurrentLyrics { get; private set; }
        /// <summary>
        /// 返回下一个歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string NextLyrics { get; private set; }
        /// <summary>
        /// 返回上一个歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string PreviousLyrics { get; private set; }
        /// <summary>
        /// 返回当前歌词的Index,使用前请先调用Refresh()函数
        /// </summary>
        public int CurrentIndex { get; private set; }
        /// <summary>
        /// 返回歌词的标题
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// 返回歌词的专辑名称
        /// </summary>
        public string Album { get; private set; }
        /// <summary>
        /// 返回歌词的表演者
        /// </summary>
        public string Artist { get; private set; }
        /// <summary>
        /// 返回歌词的制作者
        /// </summary>
        public string LyricsMaker { get; private set; }
        /// <summary>
        /// 获取LRC歌词的偏移
        /// </summary>
        public TimeSpan Offset { get; private set; }

        #endregion

        #region build

        /// <summary>
        /// 通过指定的Lrc代码初始化LyricParser实例
        /// </summary>
        /// <param name="code">Lrc代码</param>
        public Lyrics(string code)
        {
            LrcCode = code;
            LrcCodeParse();
            DictionaryParse();
            GetSortedTimes();
            CurrentIndex = -1;
        }

        #endregion

        #region protected functions

        /// <summary>
        /// 第一次处理，生成原始字典
        /// </summary>
        protected void LrcCodeParse()
        {
            Dictionary = new Dictionary<string, string>();
            string[] lines = LrcCode.Replace(@"\'", "'").Split(new char[2] { '\r', '\n' });
            int i;
            for (i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    Match mc = Regex.Match(lines[i], @"((?'titles'\[.*?\])+)(?'content'.*)", RegexOptions.None);
                    if (mc.Success)
                    {
                        string content = mc.Groups["content"].Value;
                        foreach (Capture title in mc.Groups["titles"].Captures)
                            Dictionary[title.Value] = content;		//不要用Add方法，有可能有重复项
                    }
                }
            }
        }
        /// <summary>
        /// 第二次处理，生成时间、歌词字典，找到歌词的作者等属性
        /// </summary>
        protected void DictionaryParse()
        {
            TimeAndLyrics = new Dictionary<TimeSpan, string>();
            foreach (var keyvalue in Dictionary)
            {
                {
                    //分析时间
                    Match mc = Regex.Match(keyvalue.Key, @"\[(?'minutes'\d+):(?'seconds'\d+(\.\d+)?)\]", RegexOptions.None);
                    if (mc.Success)
                    {
                        int minutes = int.Parse(mc.Groups["minutes"].Value);
                        double seconds = double.Parse(mc.Groups["seconds"].Value);
                        TimeSpan key = new TimeSpan(0, 0, minutes, (int)Math.Floor(seconds), (int)((seconds - Math.Floor(seconds)) * 1000));
                        string value = keyvalue.Value;
                        TimeAndLyrics[key] = value;
                    }
                }
                {
                    //分析歌词的附带属性
                    Match mc = Regex.Match(keyvalue.Key, @"\[(?'title'.+?):(?'content'.*)\]", RegexOptions.None);
                    if (mc.Success)
                    {
                        string title = mc.Groups["title"].Value.ToLower();
                        string content = mc.Groups["content"].Value;
                        if (title == "ti") Title = content;
                        if (title == "ar") Artist = content;
                        if (title == "al") Album = content;
                        if (title == "by") LyricsMaker = content;
                        if (title == "offset") Offset = new TimeSpan(10000 * int.Parse(content));
                    }
                }
            }
        }
        /// <summary>
        /// 从时间、歌词字典中返回排过序的时间列表
        /// </summary>
        protected void GetSortedTimes()
        {
            TimeSpan[] timesArray = new TimeSpan[TimeAndLyrics.Count];
            TimeAndLyrics.Keys.CopyTo(timesArray, 0);
            SortedTimes = new List<TimeSpan>(timesArray);
            SortedTimes.Sort();
        }

        #endregion

        #region refresh functions

        /// <summary>
        /// 使用指定的时间刷新实例的当前歌词
        /// </summary>
        /// <param name="time">时间</param>
        public void Refresh(TimeSpan time)
        {
            if (SortedTimes.Count == 0)
            {
                CurrentIndex = -1;
                PreviousLyrics = null;
                CurrentLyrics = null;
                NextLyrics = null;
            }
            else
            {
                TimeSpan time2 = time + Offset;
                while (true)
                {
                    if (CurrentIndex >= 0 && CurrentIndex < SortedTimes.Count && SortedTimes[CurrentIndex] > time2)
                        --CurrentIndex;
                    else if (CurrentIndex + 1 < SortedTimes.Count && SortedTimes[CurrentIndex + 1] <= time2)
                        ++CurrentIndex;
                    else break;
                }
                if (CurrentIndex - 1 >= 0 && CurrentIndex - 1 < SortedTimes.Count)
                    PreviousLyrics = TimeAndLyrics[SortedTimes[CurrentIndex - 1]];
                else PreviousLyrics = null;
                if (CurrentIndex >= 0 && CurrentIndex < SortedTimes.Count)
                    CurrentLyrics = TimeAndLyrics[SortedTimes[CurrentIndex]];
                else CurrentLyrics = null;
                if (CurrentIndex + 1 >= 0 && CurrentIndex + 1 < SortedTimes.Count)
                    NextLyrics = TimeAndLyrics[SortedTimes[CurrentIndex + 1]];
                else NextLyrics = null;
            }
        }

        #endregion
    }
}
