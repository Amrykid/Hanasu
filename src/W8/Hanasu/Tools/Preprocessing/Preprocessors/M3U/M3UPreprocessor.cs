using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Hanasu.Core.Utilities;

namespace Hanasu.Core.Preprocessor.Preprocessors.M3U
{
    public class M3UPreprocessor : MultiStreamPreprocessor
    {
        public async override Task<IMultiStreamEntry[]> Parse(Uri url)
        {
            var list = new List<IMultiStreamEntry>();
            string data = null;
            string[] lines = null;


            data = await HtmlTextUtility.GetHtmlFromUrl2(url.ToString(), false);
            lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);


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

            return (IMultiStreamEntry[])list.ToArray();
        }

        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(Extension);
        }

        public async override Task<Uri> Process(Uri url)
        {
            url = new Uri((await Parse(url))[0].File, UriKind.Absolute);

            return url;
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
