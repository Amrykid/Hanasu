using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;
using Hanasu.Core.Utilities;
using System.Threading.Tasks;

namespace Hanasu.Core.Preprocessor.Preprocessors.PLS
{
    public class PLSPreprocessor : MultiStreamPreprocessor
    {
        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(Extension);
        }

        public async override Task<IMultiStreamEntry[]> Parse(Uri url)
        {
            var list = new Dictionary<string, IMultiStreamEntry>();

            string str = null;
            string[] lines = null;


            str = await HtmlTextUtility.GetHtmlFromUrl2(url.ToString(), false);
            lines = str.Split('\n');




            foreach (string line in lines)
            {
                var l = line.ToLower();

                if (l.StartsWith("file"))
                {
                    l = l.Substring(4, l.IndexOf("=") - 4);

                    var file = line.Substring(line.IndexOf("=") + 1).TrimEnd('\r', '\n');

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

                    var title = line.Substring(line.IndexOf("=") + 1).TrimEnd('\r', '\n');

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
                    l = l.Substring(6, l.IndexOf("=") - 6);

                    var length = int.Parse(line.Substring(line.IndexOf("=") + 1).TrimEnd('\r', '\n'));

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



            return (IMultiStreamEntry[])list.Values.Cast<IMultiStreamEntry>().ToArray();
        }

        public override async Task<Uri> Process(Uri url)
        {
            var newurl = "";
            var str = await HtmlTextUtility.GetHtmlFromUrl2(url.ToString(), false);

            var lines = str.Split('\n');

            foreach (string line in lines)
            {
                if (line.ToLower().StartsWith("file"))
                {
                    newurl = line.Substring(line.IndexOf('=') + 1);
                    break;
                }
            }

            return new Uri(newurl);
        }

        public override bool SupportsMultiples
        {
            get { return true; }
        }

        public override string Extension
        {
            get { return ".pls"; }
        }
    }
    public struct PLSEntry : IMultiStreamEntry
    {
        public string File { get; set; }
        public string Title { get; set; }
        public int Length { get; set; }
        public int ID { get; set; }
    }
}
