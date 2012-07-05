using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections;

namespace Hanasu.Core
{
    public class HtmlTextUtility
    {
        private static Hashtable HtmlDict = null;
        static HtmlTextUtility()
        {
            HtmlDict = new Hashtable();
            HtmlDict.Add("&nbsp", " ");
            HtmlDict.Add("&quot;", "\"");
            HtmlDict.Add("&lt;", "<");
            HtmlDict.Add("&gt;", ">");
            HtmlDict.Add("&#039;", "'");
            HtmlDict.Add("&rsquo;", "’");
            HtmlDict.Add("&amp;", "&");
            HtmlDict.Add("&iuml;", "ï");
            HtmlDict.Add("&eacute;", "é");
            HtmlDict.Add("&Eacute;", "É");
            HtmlDict.Add("&Ccedil;", "Ç");
            HtmlDict.Add("&ccedil;", "ç");
        }
        public static string GetHTML(string url)
        {
            string result = null;
            using (var wc = new WebClient())
            {
                result = wc.DownloadString(url);
            }
            return result;
        }
        public static string Decode(string html)
        {
            var res = html;
            foreach (string k in HtmlDict.Keys)
            {
                res = res.Replace(k, (string)HtmlDict[k]);
            }
            return res;
        }
        public static string Encode(string html)
        {
            var res = html;
            foreach (string k in HtmlDict.Keys)
            {
                res = res.Replace((string)HtmlDict[k], k);
            }
            return res;
        }
        public static string UrlEncode(string text)
        {
            return text.Replace(" ", "%20");
        }
    }
}
