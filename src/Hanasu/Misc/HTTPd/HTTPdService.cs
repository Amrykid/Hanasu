using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

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

        private static void HandleConnection(ref TcpClient tcp)
        {
            string head = "";
            while (head.EndsWith("\r\n\r\n") == false)
                head += ReadSocket(ref tcp);

            var headLines = head.Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var firstLine = headLines[0].Split(' ');
            string method = firstLine[0], file = firstLine[1], httpversion = firstLine[2];


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
