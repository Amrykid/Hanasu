using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Collections;

namespace Hanasu.Core.Utilities
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
        public static string GetHtmlFromUrl(string url)
        {
            try
            {
                string result = null;
                using (var wc = new WebClient())
                {
                    result = wc.DownloadString(url);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetHtmlFromUrl(Uri uri)
        {
            return GetHtmlFromUrl(uri.ToString());
        }
        public static string GetHtmlFromUrl2(string url)
        {
            bool moo;
            return GetHtmlFromUrl2(url, out moo);
        }
        public static string GetHtmlFromUrl2(string url, out bool hasRedirected)
        {
            string result = null;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.ServicePoint.Expect100Continue = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11";

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                hasRedirected = res.ResponseUri.ToString() != url + "/";

                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                }
                res.Close();
            }

            hasRedirected = false;

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
        public static string UrlDecode(string text)
        {
            return text.Replace("%20", " ");
        }

        public static bool ExtensionIsWebExtension(string ext)
        {
            if (ext.StartsWith("."))
                ext = ext.Substring(1);

            switch (ext.ToLower())
            {
                case "js":
                case "htm":
                case "py":
                case "aspx":
                case "asp":
                case "html":
                case "php": return true;
                default:
                    return false;
            }
        }
    }
}
