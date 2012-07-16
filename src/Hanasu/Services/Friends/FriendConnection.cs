using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Hanasu.Core;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Hanasu.Services.Friends
{
    [Serializable]
    public class FriendConnection
    {
        public const int Port = 46318;
        public const string STATUS_CHANGED = "STATUS_CHANGED";
        public const string CHAT_MESSAGE = "CHAT_MESSAGE";
        public const string PRESENCE_ONLINE = "PRESENCE_ONLINE";
        public const string PRESENCE_OFFLINE = "PRESENCE_OFFLINE";
        public const string AVATAR_SET = "AVATAR_SET";
        internal FriendConnection(string userName, string IP, int KEY)
        {
            UserName = userName;
            IPAddress = IP;
            Key = KEY;
            Initiate(IP);
        }

        public void Initiate(string IP)
        {
            // Socket = new UdpClient();
            EndPoint = new IPEndPoint(System.Net.IPAddress.Parse(IP), 46318);

            //Socket.Connect(EndPoint);

            IsConnected = true;

        }

        public string UserName { get; private set; }
        internal IPEndPoint EndPoint = null;
        public int Key { get; private set; }
        public string IPAddress { get; private set; }

        public bool IsConnected { get; private set; }

        public string AvatarUrl { get; set; }

        //private UdpClient Socket = null;

        [NonSerialized]
        private string _status = "Offline";
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
        public static readonly System.Windows.DependencyProperty StatusProperty = System.Windows.DependencyProperty.Register("Status", typeof(string), typeof(FriendConnection));

        private void SendRaw(string msg)
        {
            var data = System.Text.UnicodeEncoding.Unicode.GetBytes(msg);
            Hanasu.Services.Friends.FriendsService.GlobalSocket.Send(data, data.Length, EndPoint);
        }
        public void SendData(string data, string type = "NOTIF")
        {
            SendRaw(
                String.Format("{0} {1} :{2}", type, Key.ToString(), data));
        }
        public void SendStatusChange(string status)
        {
            SendData(status, STATUS_CHANGED);
        }
        public void SendChatMessage(string msg)
        {
            SendData(msg, CHAT_MESSAGE);
        }
        public void SetPresence(bool isOnline = true)
        {
            if (isOnline)
                SendData("ONLINE", PRESENCE_ONLINE);
            else
                SendData("OFFLINE", PRESENCE_OFFLINE);
        }
        public void SetAvatar(string avatarurl)
        {
            SendData(avatarurl, AVATAR_SET);
        }
        /*public void SendObject(string type, object obj)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                var data = System.Text.UnicodeEncoding.Unicode.GetBytes(
                    String.Format("{0} {1} :", type, Key.ToString()));

                var objbits = ms.ToArray();

                byte[] rv = new byte[data.Length + objbits.Length];
                System.Buffer.BlockCopy(data, 0, rv, 0, data.Length);
                System.Buffer.BlockCopy(objbits, 0, rv, data.Length, objbits.Length);

                Hanasu.Services.Friends.FriendsService.GlobalSocket.Send(rv, rv.Length, EndPoint);
            }
        }*/

        [NonSerialized]
        public bool IsOnline = false;
    }
}
