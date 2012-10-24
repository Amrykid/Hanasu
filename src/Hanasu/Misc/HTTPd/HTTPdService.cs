using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Windows;
using System.IO;

namespace Hanasu.Misc.HTTPd
{
    public static class HTTPdService
    {
        private static TcpListener tcpListener = new TcpListener(IPAddress.Any, 4477);

        public static void Start()
        {
            if (IsRunning) throw new InvalidOperationException();

            try
            {
                tcpListener.Start();

                IsRunning = true;

                tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleRequest), null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void HandleRequest(IAsyncResult res)
        {
            try
            {
                var tcp = tcpListener.EndAcceptTcpClient(res);

                if (IsRunning)
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleRequest), null);

                HandleConnection(tcp);
            }
            catch (Exception)
            {
                if (IsRunning)
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleRequest), null);
            }
        }

        private const string BaseDir = "/Misc/HTTPd/Web App/Hanasu-Web-App";

        private static void HandleConnection(TcpClient tcp)
        {
            try
            {
                string head = "";

                while (head.EndsWith("\r\n\r\n") == false)
                {
                    head += ReadSocket(ref tcp);

                    if (head == "") return;
                }

                var headLines = head.Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var firstLine = headLines[0].Split(' ');
                string method = firstLine[0].ToUpper(), file = firstLine[1], httpversion = firstLine[2];

                bool keepalive = headLines.First(t => t.StartsWith("Connection: ")) == null ?
                    false :
                    headLines.First(t => t.StartsWith("Connection: ")).Substring("Connection: ".Length).ToLower() == "keep-alive";

                string host = headLines.First(t => t.StartsWith("Host: ")).Substring("Host :".Length);

                bool close = !keepalive;

                if (method == "GET" || method == "HEAD")
                {
                    #region GET/HEAD
                    //hard coded atm
                    //TODO: make this not hard-coded.

                    var fileToGet = "";
                    string[] queryVars = null;

                    if (file.Contains("?"))
                    {
                        var fileSplit = file.Split(new char[] { '?' }, 2);

                        file = fileSplit[0];

                        queryVars = fileSplit[1].Split('&');
                    }

                    switch (file.ToLower())
                    {
                        case "/":
                            {
                                fileToGet = "/index.html";
                            }
                            break;
                        default:
                            fileToGet = file;
                            break;
                    }

                    //find if compression is supported
                    var compressionSupported = headLines.First(t => t.ToLower().StartsWith("accept-encoding")).Split(',');
                    compressionSupported[0] = compressionSupported[0].Substring("accept-encoding: ".Length);
                    //only ones that would be supported is deflate (default) and gzip

                    bool useCompression = compressionSupported.Any(t => t.ToLower() == "deflate") || compressionSupported.Any(t => t.ToLower() == "gzip");
                    string compressionToUse = null;
                    string compressionHeader = null;

                    if (compressionSupported.Contains("deflate"))
                        compressionToUse = "deflate";
                    else if (compressionSupported.Contains("gzip"))
                        compressionToUse = "gzip";

                    if (compressionToUse != null)
                        compressionHeader = "Content-Encoding: " + compressionToUse;

                    try
                    {
                        if (method == "GET")
                        {

                            if (_urlHandlers.ContainsKey(fileToGet))
                            {
                                if (_urlHandlers[fileToGet].Item1 == HttpRequestType.GET || _urlHandlers[fileToGet].Item1 == HttpRequestType.GETANDPOST)
                                {
                                    if (HttpUrlHandler != null)
                                    {
                                        var output = HttpUrlHandler(fileToGet, HttpRequestType.GET, queryVars, null).ToString();

                                        WriteSocket(ref tcp,
                                            HttpResponseBuilder.OKResponse(output, host, HttpMimeTypes.Html, close, false, compressionHeader), output, close, new Tuple<bool, string>(useCompression, compressionToUse));
                                    }
                                }
                                else
                                {
                                    WriteSocket(ref tcp, HttpResponseBuilder.MethodNotAllowedResponse(host), close);
                                }
                            }
                            else
                            {
                                //is a included file and not a handler.

                                var s = Application.GetResourceStream(
                                    new Uri(@BaseDir + fileToGet, UriKind.RelativeOrAbsolute));
                                var mimeType = s.ContentType;

                                if (fileToGet.EndsWith(".woff"))
                                    mimeType = "application/font-woff"; //may need to make this like urlhandlers where you can register them.

                                if (mimeType.StartsWith("text"))
                                    using (var sr = new System.IO.StreamReader(s.Stream))
                                    {
                                        if (fileToGet.EndsWith(".js"))
                                            mimeType = HttpMimeTypes.Javascript;
                                        else if (fileToGet.EndsWith(".css"))
                                            mimeType = HttpMimeTypes.Css;

                                        var data = sr.ReadToEnd();

                                        WriteSocket(ref tcp,
                                            HttpResponseBuilder.OKResponse(data, host, mimeType, close, false, compressionHeader),
                                            data, close, new Tuple<bool, string>(useCompression, compressionToUse)); //for text documents

                                        sr.Close();
                                    }
                                else
                                {
                                    //for non text-based files

                                    List<byte> bytes = new List<byte>();

                                    int byt;

                                    while ((byt = s.Stream.ReadByte()) > -1)
                                    {
                                        bytes.Add((byte)byt);
                                    }

                                    var dataBytes = bytes.ToArray();

                                    WriteSocket(ref tcp, HttpResponseBuilder.OKResponse(dataBytes, host, mimeType, close), false);
                                    WriteSocket(ref tcp, dataBytes, close);

                                }

                                s.Stream.Close();
                            }
                        }
                        else if (method == "HEAD")
                        {
                            WriteSocket(ref tcp, HttpResponseBuilder.OKResponse("", host, HttpMimeTypes.Html, close), close);
                        }


                    }
                    catch (Exception)
                    {
                        WriteSocket(ref tcp, HttpResponseBuilder.NotFoundResponse(host), close);
                    }
                    #endregion
                }
                else if (method == "POST")
                {
                    string[] queryVars = null;

                    if (file.Contains("?"))
                    {
                        var fileSplit = file.Split(new char[] { '?' }, 2);

                        file = fileSplit[0];

                        queryVars = fileSplit[1].Split('&');
                    }

                    if (HttpPostReceived != null)
                        HttpPostReceived(file, null);

                    if (_urlHandlers.ContainsKey(file))
                    {
                        if (_urlHandlers[file].Item1 == HttpRequestType.POST || _urlHandlers[file].Item1 == HttpRequestType.GETANDPOST)
                        {
                            string[] postData = null; // to be implemented

                            if (HttpUrlHandler != null)
                                WriteSocket(ref tcp, HttpResponseBuilder.OKResponse(HttpUrlHandler(file, HttpRequestType.POST, queryVars, postData).ToString(), host, HttpMimeTypes.Html, close), close);
                        }
                        else
                        {
                            WriteSocket(ref tcp, HttpResponseBuilder.MethodNotAllowedResponse(host), close);
                        }
                    }
                    else
                    {
                        WriteSocket(ref tcp, HttpResponseBuilder.NotFoundResponse(host), close);
                    }

                }

                if (keepalive)
                    HandleConnection(tcp);
            }
            catch (Exception)
            {
                tcp.Close();
            }


        }
        private static void WriteSocket(ref TcpClient tcp, string data, bool close = true)
        {
            try
            {
                var bits = System.Text.UTF8Encoding.UTF8.GetBytes(data);

                tcp.Client.Send(bits);
            }
            catch (Exception)
            {
            }

            if (close)
                tcp.Close();
        }
        private static void WriteSocket(ref TcpClient tcp, string headers, string data, bool close = true, Tuple<bool, string> compressionScheme = null)
        {
            try
            {
                var headerBits = System.Text.UTF8Encoding.UTF8.GetBytes(headers);
                tcp.Client.Send(headerBits);


                var bits = System.Text.UTF8Encoding.UTF8.GetBytes(data);

                if (compressionScheme != null && compressionScheme.Item1 == true)
                {
                    if (compressionScheme.Item2.ToLower() == "deflate")
                        bits = CompressData_Deflate(bits);
                    else if (compressionScheme.Item2.ToLower() == "gzip")
                        bits = CompressData_GZip(bits);
                }

                tcp.Client.Send(bits);
            }
            catch (Exception)
            {
            }

            if (close)
                tcp.Close();
        }
        private static void WriteSocket(ref TcpClient tcp, Byte[] data, bool close = true)
        {
            try
            {
                tcp.Client.Send(data);
            }
            catch (Exception)
            {
            }

            if (close)
                tcp.Close();
        }

        private static byte[] CompressData_Deflate(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress);
            ds.Write(data, 0, data.Length);
            ds.Close();
            ds.Dispose();
            byte[] compressed = (byte[])ms.ToArray();
            ms.Close();
            ds.Dispose();
            return compressed;
        }
        private static byte[] CompressData_GZip(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            System.IO.Compression.GZipStream ds = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress);
            ds.Write(data, 0, data.Length);
            ds.Close();
            ds.Dispose();
            byte[] compressed = (byte[])ms.ToArray();
            ms.Close();
            ms.Dispose();
            return compressed;
        }

        private static string ReadSocket(ref TcpClient tcp)
        {
            tcp.Client.ReceiveTimeout = 100;

            //This is used for converting bytes to strings.
            byte[] b = new byte[100];
            int k = tcp.Client.Receive(b);
            StringBuilder msg = new StringBuilder();
            for (int i = 0; i < k; i++)
                msg.Append(Convert.ToChar(b[i]));
            return msg.ToString();
        }

        public static void Stop()
        {
            if (!IsRunning) throw new InvalidOperationException();

            try
            {
                tcpListener.Stop();

                IsRunning = false; ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsRunning { get; private set; }

        public delegate void HttpPostReceivedHandler(string file, object postdata);
        public static event HttpPostReceivedHandler HttpPostReceived;

        public delegate object HttpUrlHandlerHandler(string relativeUrl, HttpRequestType type, string[] queryVars, string[] postdata);
        public static event HttpUrlHandlerHandler HttpUrlHandler;

        public static void RegisterUrlHandler(string relativeUrl, HttpRequestType type, string helpInformation)
        {
            if (_urlHandlers.ContainsKey(relativeUrl)) throw new Exception("Url is already registered.");

            _urlHandlers.Add(relativeUrl, new Tuple<HttpRequestType, string>(type, helpInformation));
        }

        private static Dictionary<string, Tuple<HttpRequestType, string>> _urlHandlers = new Dictionary<string, Tuple<HttpRequestType, string>>();

        public static IEnumerable<KeyValuePair<string, Tuple<HttpRequestType, string>>> GetUrlHandlers()
        {
            return _urlHandlers.ToArray();
        }

        public enum HttpRequestType
        {
            GET = 0,
            HEAD = 1,
            POST = 2,
            GETANDPOST = 3
        }
    }
}
