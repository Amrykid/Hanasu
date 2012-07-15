using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Hanasu.Core;

namespace Hanasu.Services.Friends
{
    [Serializable]
    public class FriendConnection
    {
        public const int Port = 46318;
        internal FriendConnection(string userName, string IP, int KEY)
        {
            UserName = userName;
            IPAddress = IP;
            Key = KEY;
            Initiate(IP);
        }

        public void Initiate(string IP)
        {
            Socket = new UdpClient(Port, AddressFamily.InterNetwork);
            //Socket.ExclusiveAddressUse = false;
            EndPoint = new IPEndPoint(System.Net.IPAddress.Parse(IP), 46318);
            Socket.Connect(EndPoint);
            IsConnected = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
            {
                HandleConnection();
            }));
        }
        public string UserName { get; private set; }
        private IPEndPoint EndPoint = null;
        public int Key { get; private set; }
        public string IPAddress { get; private set; }

        public bool IsConnected { get; private set; }

        [NonSerialized]
        private UdpClient Socket = null;

        [NonSerialized]
        private string _status = null;
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private bool PollForData()
        {
            return Socket.Available > 0;
        }
        private void HandleConnection()
        {
            try
            {
                if (PollForData())
                {
                    ReadData();
                }

                Thread.Sleep(5000);

                HandleConnection();
            }
            catch (Exception)
            {
                IsConnected = false;
                return;
            }
        }
        private void ReadData()
        {
            try
            {
                var data = Socket.Receive(ref EndPoint);
                var str = System.Text.ASCIIEncoding.ASCII.GetString(data);

                var spl = str.Split(new char[] { ' ' }, 3);

                var sentKey = int.Parse(spl[1]);
                if (sentKey == Key)
                {
                    Hanasu.Services.Friends.FriendsService.Instance.HandleReceievedData(this, spl[0], spl[2].Substring(1));
                }
                else
                    return;

            }
            catch (Exception)
            {
            }
        }
        private void SendRaw(string msg)
        {
            var data = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            Socket.Send(data, data.Length);
        }
        public void SendData(string data, string type = "NOTIF")
        {
            SendRaw(
                String.Format("{0} {1} :{2}", type, Key.ToString(), data));
        }
    }
}
