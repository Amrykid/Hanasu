using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Misc.HTTPd
{
    public static class HttpResponseBuilder
    {

        public static string OKResponse(string fileText, string host, string mimetype = "text/html; charset=UTF-8", bool close = true)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "200 OK", host);

            if (close)
                sb.AppendLine("Connection: close");
            else
                sb.AppendLine("Connection: keep-alive");

            sb.AppendLine("Content-Type: " + mimetype + "; charset=UTF-8");
            sb.AppendLine("Contenth-Length: " + fileText.Length);

            sb.AppendLine();
            sb.Append(fileText);

            return sb.ToString();
        }

        public static string TemporaryRedirectResponse(string url, string host)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "307 Temporary Redirect", host);

            sb.AppendLine("Location: " + url);
            sb.AppendLine("Connection: close");

            sb.AppendLine();

            return sb.ToString();
        }

        private static void GenerateStandardHeaders(ref StringBuilder sb, string p, string host)
        {
            sb.AppendLine("HTTP/1.1 " + p);
            sb.AppendLine("Date: " + DateTime.UtcNow.ToLongTimeString());
            sb.AppendLine("Server: Hanasu 2.0 HTTPd");
            sb.AppendLine("Host: " + host);
        }


        public static string NoContentResponse(string host)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "204 No Content", host);

            sb.AppendLine("Connection: close");

            sb.AppendLine();

            return sb.ToString();
        }

        public static string MethodNotAllowedResponse(string host)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "405 Method Not Allowed", host);

            sb.AppendLine("Connection: close");

            sb.AppendLine();

            return sb.ToString();
        }

        public static string NotFoundResponse(string host)
        {
            StringBuilder sb = new StringBuilder();
            GenerateStandardHeaders(ref sb, "404 Not Found", host);

            sb.AppendLine("Connection: close");

            sb.AppendLine();

            return sb.ToString();
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
