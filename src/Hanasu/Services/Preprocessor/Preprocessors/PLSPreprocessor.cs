using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;

namespace Hanasu.Services.Preprocessor.Preprocessors
{
    public class PLSPreprocessor : BasePreprocessor
    {
        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(".pls");
        }

        public PLSEntry[] Parse(Uri url)
        {
            var list = new Hashtable();

            using (WebClient wc = new WebClient())
            {
                var str = wc.DownloadString(url);
                var lines = str.Split('\n');

                foreach (string line in lines)
                {
                    var l = line.ToLower();

                    if (l.StartsWith("file"))
                    {
                        l = l.Substring(4, l.IndexOf("=") - 4);

                        var file = line.Substring(line.IndexOf("=") + 1);

                        if (list.ContainsKey(l))
                            ((dynamic)list[l]).File = file;
                        else
                        {
                            list.Add(l,
                                new PLSEntry()
                                {
                                    File = file
                                });
                        }
                    }
                    else if (l.StartsWith("title"))
                    {
                        l = l.Substring(5, l.IndexOf("=") - 5);

                        var title = line.Substring(line.IndexOf("=") + 1);

                        if (list.ContainsKey(l))
                            ((dynamic)list[l]).Title = title;
                        else
                        {
                            list.Add(l,
                                new PLSEntry()
                                {
                                    Title = title
                                });
                        }
                    }
                    else if (l.StartsWith("length"))
                    {
                        l = l.Substring(5, l.IndexOf("=") - 5);

                        var length = int.Parse(line.Substring(line.IndexOf("=") + 1));

                        if (list.ContainsKey(l))
                            ((dynamic)list[l]).Length = length;
                        else
                        {
                            list.Add(l,
                                new PLSEntry()
                                {
                                    Length = length
                                });
                        }
                    }
                }
            }

            return list.Values.Cast<PLSEntry>().ToArray();
        }

        public override void Process(ref Uri url)
        {
            var newurl = "";
            using (WebClient wc = new WebClient())
            {
                var str = wc.DownloadString(url);

                var lines = str.Split('\n');

                foreach (string line in lines)
                {
                    if (line.ToLower().StartsWith("file"))
                    {
                        newurl = line.Substring(line.IndexOf('=') + 1);
                        break;
                    }
                }
            }

            url = new Uri(newurl);
        }

        public override bool SupportsMultiples
        {
            get { return true; }
        }
    }
    public struct PLSEntry
    {
        public string File { get; set; }
        public string Title { get; set; }
        public int Length { get; set; }
        public int ID { get; set; }
    }
}
