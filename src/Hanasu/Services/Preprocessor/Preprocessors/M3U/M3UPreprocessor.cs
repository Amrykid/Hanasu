using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace Hanasu.Services.Preprocessor.Preprocessors.M3U
{
    public class M3UPreprocessor : MultiStreamPreprocessor
    {
        public override IMultiStreamEntry[] Parse(Uri url)
        {
            var list = new ArrayList();
            string data = null;
            string[] lines = null;

            if (url.Host != "")
                using (WebClient wc = new WebClient())
                {
                    data = wc.DownloadString(url);
                    lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            else
                lines = File.ReadAllLines(url.LocalPath);


            bool m3uIsValid = true;

            if (!lines[0].StartsWith("#EXTM3U"))
                m3uIsValid = false;

            if (m3uIsValid)
            {
                var item = new M3UEntry();
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.StartsWith("#EXTINF"))
                    {
                        item = new M3UEntry();

                        var m3udata = line.Substring(8);

                        item.Length = int.Parse(m3udata.Substring(0, m3udata.IndexOf(",")));

                        item.Title = m3udata.Substring(m3udata.IndexOf(",") + 1).TrimEnd('\r', '\n');
                    }
                    else
                    {
                        item.File = line;
                        list.Add(item);
                    }
                }
            }
            else
            {
                foreach (string line in lines)
                {
                    var item = new M3UEntry();
                    item.Title = line;
                    item.File = line;

                    list.Add(item);
                }
            }

            return (IMultiStreamEntry[])list.ToArray(typeof(IMultiStreamEntry));
        }

        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(Extension);
        }

        public override void Process(ref Uri url)
        {
            return;
        }

        public override string Extension
        {
            get { return ".m3u"; }
        }
    }
    public struct M3UEntry : IMultiStreamEntry
    {
        public string File { get; set; }
        public string Title { get; set; }
        public int Length { get; set; }
        public int ID { get; set; }
    }
}
