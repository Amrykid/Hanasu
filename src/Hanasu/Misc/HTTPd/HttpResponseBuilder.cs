using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Misc.HTTPd
{
    public static class HttpResponseBuilder
    {

        public static string OKResponse(string fileText,string mimetype = "text/html; charset=ASCII", bool close = true)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "100 OK");

            if (close)
                sb.AppendLine("Connection: close");
            else
                sb.AppendLine("Connection: keep-alive");

            sb.AppendLine("Content-Type: " + mimetype + "; charset=ASCII");
            sb.AppendLine("Contenth-Length: " + fileText.Length);

            sb.AppendLine();
            sb.Append(fileText);

            return sb.ToString();
        }

        public static string TemporaryRedirectResponse(string url)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "307 Temporary Redirect");

            sb.AppendLine("Location: " + url);
            sb.AppendLine("Connection: close");

            sb.AppendLine();

            return sb.ToString();
        }

        private static void GenerateStandardHeaders(ref StringBuilder sb, string p)
        {
            sb.AppendLine("HTTP/1.1 " + p);
            sb.AppendLine("Date: " + DateTime.UtcNow.ToLongTimeString());
            sb.AppendLine("Server: Hanasu 2.0 HTTPd");
        }


    }

    public static class HttpMimeTypes
    {
        public const string Html = "text/html";
        public const string Plain = "text/plain";
        public const string Xml = "text/xml";
        public const string Gif = "image/gif";
        public const string Jpg = "image/jpeg";
        public const string Png = "image/png";
        public const string Tiff = "image/tiff";
        public const string Css = "text/css";
        public const string Javascript = "text/javascript"; //"application/javascript";
    }
}
