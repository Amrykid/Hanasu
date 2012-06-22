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
        /// <summary>
        /// Gets the title from the html code.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetTitleFromRaw(string html)
        {
            Regex r = new Regex("<title>.+?</title>");
            string title = r.Match(html).Value.Replace("<title>", "").Replace("</title>", "");
            return title;
        }
        public static string GetTitleFromURL(string url)
        {
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
            //hwr.Method = "GET";
            //hwr.KeepAlive = false;
            //hwr.ImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.None;
            //hwr.UserAgent = "AmryBot 0.1";
            HttpWebResponse res = (HttpWebResponse)hwr.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string html = sr.ReadToEnd();
            return GetTitleFromRaw(html);
        }
        public static string GetHTML(string url)
        {/*
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
            hwr.Method = "GET";
            //hwr.KeepAlive = false;
            hwr.ImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            hwr.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/525.13 (KHTML, like Gecko) Chrome/0.A.B.C Safari/525.13";
            HttpWebResponse res = (HttpWebResponse)hwr.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string html = sr.ReadToEnd();
            return html; */

            string html = null;
            using (WebClient wc = new WebClient())
            {
                html = wc.DownloadString(url);
            }
            return html;
        }
        public static string Decode(string html)
        {
            return html.Replace("&nbsp", " ").Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">");
        }
    }
}
