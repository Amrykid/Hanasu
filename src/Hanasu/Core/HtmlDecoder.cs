using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Hanasu.Core
{
    //Old code I wrote years ago. Dont mind it.
    public class HtmlDecoder
    {
        public static string Decode(string html)
        {
            return html.Replace("&nbsp", " ").Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#039;", "'").Replace("&rsquo;", "’").Replace("&amp;", "&").Replace("&iuml;", "ï");
        }
    }
}
