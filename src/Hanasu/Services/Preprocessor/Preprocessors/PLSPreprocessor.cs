using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Hanasu.Services.Preprocessor.Preprocessors
{
    public class PLSPreprocessor: BasePreprocessor
    {
        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(".pls");
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
    }
}
