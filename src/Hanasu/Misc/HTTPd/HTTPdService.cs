using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Windows;

namespace Hanasu.Misc.HTTPd
{
    public static class HTTPdService
    {
        private static TcpListener tcpListener = new TcpListener(IPAddress.Any, 80);

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
            var tcp = tcpListener.EndAcceptTcpClient(res);

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleRequest), null);

            HandleConnection(ref tcp);


        }

        private const string BaseDir = "/Misc/HTTPd/Web App/Hanasu-Web-App";

        private static void HandleConnection(ref TcpClient tcp)
        {
            try
            {
                string head = "";
                while (head.EndsWith("\r\n\r\n") == false)
                    head += ReadSocket(ref tcp);

                var headLines = head.Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var firstLine = headLines[0].Split(' ');
                string method = firstLine[0].ToUpper(), file = firstLine[1], httpversion = firstLine[2];

                if (method == "GET")
                {
                    //hard coded atm

                    var fileToGet = "";
                    bool close = true;
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

                    var s = Application.GetResourceStream(
                        new Uri(@BaseDir + fileToGet, UriKind.RelativeOrAbsolute));
                    using (var sr = new System.IO.StreamReader(s.Stream))
                    {
                        WriteSocket(ref tcp, HttpResponseBuilder.OKResponse(sr.ReadToEnd(), fileToGet.EndsWith(".js") ? HttpMimeTypes.Javascript : s.ContentType));

                        sr.Close();
                    }
                }
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
                tcp.Client.Send(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(data));
            }
            catch (Exception)
            {
            }

            if (close)
                tcp.Close();
        }
        private static string ReadSocket(ref TcpClient tcp)
        {
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
    }
}
