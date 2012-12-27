using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Hanasu.Tools.Shoutcast
{
    //http://forums.radiotoolbox.com/viewtopic.php?t=74

    public class ShoutcastStream : IRandomAccessStream
    {
        private Stream underlayingStream = null;
        //private HttpClient underlayingClient = null;
        private StreamSocket underlayingSocket = null;
        private Stream outStream = null;
        internal ShoutcastStream()
        {

        }

        internal async Task ConnectAsync(Uri url)
        {
            underlayingSocket = new StreamSocket();
            await underlayingSocket.ConnectAsync(remoteHostName: new Windows.Networking.HostName(url.Host), remoteServiceName: url.Port.ToString());
            await SendHeaders(url);
            await SetStream();

            //underlayingClient.DefaultRequestHeaders.Add("icy-metadata", "1");
            //underlayingClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.11 (KHTML, like Gecko) Chrome/20.0.1132.57 Safari/536.11");

            //await SetStream(url);
        }

        private async Task SendHeaders(Uri url)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GET / HTTP/1.1");
            sb.AppendLine("Connection: keep-alive");
            sb.AppendLine("Host: " + url.Host);
            sb.AppendLine("icy-metadata: 0");
            sb.AppendLine();

            outStream = underlayingSocket.OutputStream.AsStreamForWrite();

            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(sb.ToString());

            await outStream.WriteAsync(bytes, 0, bytes.Length);
            await outStream.FlushAsync();
        }

        private async Task SetStream()
        {
            underlayingStream = underlayingSocket.InputStream.AsStreamForRead();

            string headerinfo = string.Empty;

            while (true)
            {
                headerinfo += await ReadLine();

                if (headerinfo.EndsWith("\r\n\r\n")) break;
            }

            var h = headerinfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (h[0].Contains("Found") && h[0].StartsWith("HTTP"))
            {
                Dispose();

                foreach (var header in h)
                    if (header.StartsWith("Location:"))
                    {
                        var newUrl = header.Split(new char[] { ':' }, 2)[1].TrimStart(' ');
                        await ConnectAsync(new Uri(newUrl));
                        break;
                    }
            }
            else if (h[0].ToLower().StartsWith("icy"))
            {
                //we got a icy stream now!

                foreach (var header in h.Skip(1))
                {
                    var split = header.Split(new char[] { ':' }, 2);
                    var name = split[0];
                    var value = split[1];

                    switch (name.ToLower())
                    {
                        case "icy-name":
                            Icy_Name = value;
                            break;
                        case "icy-genre":
                            Icy_Genre = value;
                            break;
                        case "icy-url":
                            Icy_Url = value;
                            break;
                        case "content-type":
                            Content_Type = value;
                            break;
                        case "icy-pub":
                            Icy_Pub = int.Parse(value);
                            break;
                        case "icy-metaint":
                            Icy_Metaint = int.Parse(value);
                            break;
                        case "icy-br":
                            Icy_Br = int.Parse(value);
                            break;
                    }
                }

                var tag = underlayingStream.ReadByte();
                MetaDataTagLength = tag * 16;
            }
        }

        public string Icy_Name { get; private set; }
        public string Icy_Genre { get; private set; }
        public string Icy_Url { get; private set; }
        public string Content_Type { get; private set; }
        public int Icy_Pub { get; private set; }
        public int Icy_Metaint { get; private set; }
        public int Icy_Br { get; private set; }
        public int MetaDataTagLength { get; private set; }

        private async Task<string> ReadLine()
        {
            StringBuilder msg = new StringBuilder();
            while (true)
            {
                byte[] b = new byte[1];
                int k = await underlayingStream.ReadAsync(b, 0, b.Length);

                char c = Convert.ToChar(b[0]);
                msg.Append(c);

                if (msg.ToString().EndsWith("\r\n")) break;
            }
            return msg.ToString();
        }



        public IRandomAccessStream CloneStream()
        {
            throw new NotImplementedException();
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }


        private ulong _metapos = 0;
        private ulong _pos = 0;
        public ulong Position
        {
            get { return _pos; }
        }

        public void Seek(ulong position)
        {
            bytesToSkip = (int)position;

            return;
        }

        public ulong Size
        {
            get
            {
                return  _pos == 0 ? 100000 : _pos * 4;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private int bytesToSkip = 0;

        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            if (Icy_Metaint == 0) //title streaming is disabled
            {
                _pos += count;
                return underlayingSocket.InputStream.ReadAsync(buffer, count, InputStreamOptions.None);
            }
            else
            {
                if ((ulong)Icy_Metaint > _metapos + count)
                {
                    _pos += count;

                    return underlayingSocket.InputStream.ReadAsync(buffer, count, options);
                }
                else
                {
                    #region try to handle stream and extract title
                    for (int i = 0; i < bytesToSkip; i++)
                    {
                        underlayingStream.ReadByte();
                        _pos += (uint)i;
                    }
                    bytesToSkip = 0;

                    var x = (int)count;

                    var amountLeft = (ulong)Icy_Metaint - _metapos;

                    _pos += (uint)Icy_Metaint;

                    _metapos = 0;

                    var amountLeftInt = (uint)amountLeft;

                    int metalength = 0; //underlayingStream.ReadByte();

                    var data = underlayingSocket.InputStream.ReadAsync(new Windows.Storage.Streams.Buffer((uint)Icy_Metaint), (uint)Icy_Metaint, InputStreamOptions.None);

                    if (metalength != 0)
                    {
                        //ParseTitle();

                        //var calc = (uint)Icy_Metaint - (uint)MetaDataTagLength + 1;

                        //_pos += calc;

                        //return underlayingSocket.InputStream.ReadAsync(
                        //    new Windows.Storage.Streams.Buffer(calc), 
                        //    calc, InputStreamOptions.None);
                    }

                    return data;

                    #endregion
                }
            }

            return null;
        }

        private string ParseTitle()
        {
            byte[] titleBuff = new byte[MetaDataTagLength];
            var moo = underlayingStream.Read(titleBuff, 0, titleBuff.Length);

            StringBuilder msg = new StringBuilder();

            for (int i = 0; i < moo; i++)
                msg.Append(Convert.ToChar(titleBuff[i]));
            var msg1 = msg.ToString();

            var msg2 = System.Text.UTF8Encoding.UTF8.GetString(titleBuff, 0, titleBuff.Length);

            return msg1 + msg2;
        }

        public new Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return false; }
        }

        public void Dispose()
        {
            _pos = 0;
            _metapos = 0;

            outStream.Dispose();
            underlayingStream.Dispose();
            underlayingSocket.Dispose();
        }
    }
}
