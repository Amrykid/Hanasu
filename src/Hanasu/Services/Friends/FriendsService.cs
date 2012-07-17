using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Data;
using Hanasu.Windows;
using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Hanasu.Services.Events;
using System.Xml.Linq;
using Hanasu.Services.Song;
using System.Threading.Tasks;

namespace Hanasu.Services.Friends
{
    public class FriendsService : BaseINPC, IStaticService
    {
        static FriendsService()
        {
            if (!IsInitialized)
                Initialize();
        }
        public static void Initialize()
        {
            if (IsInitialized) return;

            Instance = new FriendsService();

            var dir = Hanasu.Services.Stations.StationsService.StationsCacheDir;

            Instance.FriendsDBFile = dir.Replace("\\Cache", "") + "Friends.bin";

            System.Windows.Application.Current.Exit += Current_Exit;

            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Settings_Created, HandleSettingsCreated);
            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Settings_Loaded, HandleSettingsLoaded);
            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Settings_Saving, HandleSettingsSaving);

            /*if (UPnP.NAT.Discover())
            {
                Hanasu.Services.Notifications.NotificationsService.AddNotification("Network Info", "Your external ip is " + UPnP.NAT.GetExternalIP(), 7000, true);
            }*/

            LoadFriends();

            InitializeSocket();

            IsInitialized = true;
        }

        private static void InitializeSocket()
        {
            try
            {
                InitializeSocketUDP();
                InitializeSocketTCP();

                BroadcastPresence(true);
                //BroadcastStatus("Online - Idle");
                Hanasu.Services.Events.EventService.AttachHandler(EventType.Station_Player_Idle,
                    e =>
                    {
                        BroadcastStatus("Idle", false);
                    });
                Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Station_Changed,
                    e =>
                    {
                        var e2 = (Hanasu.MainWindow.StationEventInfo)e;

                        switch (e2.CurrentStation.StationType)
                        {
                            case Stations.StationType.Radio: BroadcastStatus("Listening to " + e2.CurrentStation.Name, false);
                                break;
                            case Stations.StationType.TV: BroadcastStatus("Watching " + e2.CurrentStation.Name, false);
                                break;
                        }

                    });
            }
            catch (SocketException)
            {
                isRunningUDP = false;

                Hanasu.Services.Notifications.NotificationsService.AddNotification("Friends Service",
                    "Unable to start Friends service. Is there another instance of Hanasu running?", 5000, true, Notifications.NotificationType.Error);
            }
        }


        private static void BroadcastPresence(bool isOnline, bool onlyUDP = true)
        {
            if (!isRunningUDP) return;

            if (onlyUDP)
            {
                foreach (var con in Instance.Friends.Where(e => e.Connection.IsUDP == true))
                    if (con.Connection.IsConnected)
                        con.Connection.SetPresence(isOnline);
            }
            else
                foreach (var con in Instance.Friends)
                    if (con.Connection.IsConnected)
                        con.Connection.SetPresence(isOnline);
        }
        private static void BroadcastAvatar(string url, bool onlyUDP = true)
        {
            if (!isRunningUDP) return;

            if (string.IsNullOrEmpty(url)) return;

            if (onlyUDP)
            {
                foreach (var con in Instance.Friends.Where(e => e.Connection.IsUDP == true))
                    if (con.Connection.IsConnected)
                        con.Connection.SetAvatar(url);
            }
            else
                foreach (var con in Instance.Friends)
                    if (con.Connection.IsConnected)
                        con.Connection.SetAvatar(url);

        }
        private static void BroadcastStatus(string status, bool onlyUDP = true)
        {
            if (!isRunningUDP) return;

            if (string.IsNullOrEmpty(status)) return;

            if (onlyUDP)
            {
                foreach (var con in Instance.Friends.Where(e => e.Connection.IsUDP == true))
                    if (con.Connection.IsConnected)
                        con.Connection.SendStatusChange(status);
            }
            else
                foreach (var con in Instance.Friends)
                    if (con.Connection.IsConnected)
                        con.Connection.SendStatusChange(status);
        }


        private static void HandleSettingsCreated(EventInfo ei)
        {
            Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo sdei = (Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo)ei;
        }
        private static void HandleSettingsLoaded(EventInfo ei)
        {
            Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo sdei = (Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo)ei;

            AvatarUrl = sdei.SettingsElement.ContainsElement("AvatarUrl") ? sdei.SettingsElement.Element("AvatarUrl").Value : null;
            //BroadcastAvatar(_avatarurl);

        }
        private static void HandleSettingsSaving(EventInfo ei)
        {
            Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo sdei = (Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo)ei;

            sdei.SettingsElement.Add(
                new XElement("AvatarUrl", AvatarUrl));
        }

        #region Socket
        #region UDP
        private static void InitializeSocketUDP()
        {
            GlobalSocketUDP = new UdpClient(FriendConnection.Port, AddressFamily.InterNetwork);
            isRunningUDP = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
            {
                HandleConnectionUDP();
            }));
        }
        internal static UdpClient GlobalSocketUDP { get; set; }
        private static bool isRunningUDP = false;
        private static bool PollForDataUDP()
        {
            return Hanasu.Services.Friends.FriendsService.GlobalSocketUDP.Available > 0;
        }
        private static void HandleConnectionUDP()
        {
            while (isRunningUDP)
            {
                try
                {
                    while (PollForDataUDP())
                    {
                        ReadDataUDP();
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    isRunningUDP = false;
                    return;
                }

            }
        }
        private static void ReadDataUDP()
        {
            try
            {
                IPEndPoint e = null;
                var data = GlobalSocketUDP.Receive(ref e);
                var str = System.Text.UnicodeEncoding.Unicode.GetString(data);

                var spl = str.Split(new char[] { ' ' }, 3);

                var sentKey = int.Parse(spl[1]);

                var person = Instance.Friends.First(f => f.Connection.IPAddress == e.Address.ToString());

                if (person != null)
                {
                    if (sentKey == person.Connection.Key)
                    {
                        Hanasu.Services.Friends.FriendsService.Instance.HandleReceievedData(person.Connection, spl[0], spl[2].Substring(1));
                    }
                    else
                        return;
                }

            }
            catch (Exception)
            {
            }

        }
        #endregion

        private static bool isRunningTCP = false;
        internal static TcpListener GlobalTCPListener { get; set; }
        internal static void InitializeSocketTCP()
        {
            if (isRunningTCP == true) return;

            if (GlobalTCPListener != null) return;

            GlobalTCPListener = new TcpListener(IPAddress.Any, FriendConnection.Port);

            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
            {

                try
                {

                    GlobalTCPListener.Start();

                    isRunningTCP = true;

                }
                catch (Exception)
                {
                    isRunningTCP = false;
                    //Unable to listen.
                }

                while (isRunningTCP)
                {
                    if (GlobalTCPListener.Pending())
                    {
                        isRunningTCP = true;

                        var socket = GlobalTCPListener.AcceptTcpClient();

                        var y = socket.Client.RemoteEndPoint;

                        var ystr = y.ToString().Substring(0, y.ToString().IndexOf(":"));

                        try
                        {
                            var f = Instance.Friends.Where(z => z.Connection.IsUDP == false).First(x =>
                                x.Connection.IPAddress == ystr);
                            f.Connection.Socket = socket;

                            ThreadPool.QueueUserWorkItem(new WaitCallback(ts =>
                            {
                                f.Connection.HandleTcpConnection();
                            }));
                        }
                        catch (Exception)
                        {
                            socket.Close();
                        }
                    }
                    Thread.Sleep(2000);
                }
            }));
        }
        #endregion

        private static void LoadFriends()
        {
            if (System.IO.File.Exists(Instance.FriendsDBFile))
            {
                try
                {
                    using (var fs = new FileStream(Instance.FriendsDBFile, FileMode.OpenOrCreate))
                    {
                        IFormatter bf = new BinaryFormatter();

                        var friends = (ObservableCollection<FriendConnection>)bf.Deserialize(fs);
                        Instance.Friends = new ObservableCollection<FriendView>();

                        foreach (FriendConnection fc in friends)
                            Instance.Friends.Add(new FriendView(fc)
                                {
                                    Status = "Offline"
                                });

                        fs.Close();
                    }

                    foreach (FriendView f in Instance.Friends)
                        f.Connection.Initiate(f.Connection.IPAddress);

                    Instance.OnPropertyChanged("Friends");
                }
                catch (Exception)
                {
                    Instance.Friends = new ObservableCollection<FriendView>();
                }
            }
            else
            {
                Instance.Friends = new ObservableCollection<FriendView>();
            }
        }

        static void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            System.Windows.Application.Current.Exit -= Current_Exit;

            if (Instance.Friends.Count > 0)
            {
                if (isRunningUDP)
                    BroadcastPresence(false, false);

                using (var fs = new FileStream(Instance.FriendsDBFile, FileMode.OpenOrCreate))
                {
                    var x = new ObservableCollection<FriendConnection>();
                    foreach (var fv in Instance.Friends)
                        x.Add(fv.Connection);

                    IFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, x);
                    fs.Close();
                }
            }

            isRunningUDP = false;
        }

        private static string _avatarurl = null;
        public static string AvatarUrl
        {
            get { return _avatarurl; }
            set
            {
                _avatarurl = value;
                BroadcastAvatar(_avatarurl, false);
            }
        }

        public string ExternalIP { get { return HtmlTextUtility.GetHtmlFromUrl("http://ifconfig.me/ip"); } }

        public static bool IsInitialized { get; private set; }
        public static FriendsService Instance { get; private set; }

        public string FriendsDBFile { get; private set; }
        public ObservableCollection<FriendView> Friends { get; set; }
        private List<FriendChatWindow> ChatWindows = new List<FriendChatWindow>();

        internal void HandleReceievedData(FriendConnection friendConnection, string type, string p)
        {
            switch (type.ToUpper())
            {
                case FriendConnection.STATUS_CHANGED:
                    {

                        var view = GetFriendViewFromConnection(friendConnection);
                        view.Status = "Online - " + p;
                        friendConnection.IsOnline = true;

                        if (p.ToLower() != "idle")
                            Hanasu.Services.Notifications.NotificationsService.AddNotification(friendConnection.UserName + "'s Status",
                                p,
                                3000,
                                true,
                                Notifications.NotificationType.Now_Playing);
                    }
                    break;
                case FriendConnection.CHAT_MESSAGE:
                    {
                        Application.Current.Dispatcher.Invoke(new EmptyDelegate(() =>
                            {
                                friendConnection.IsOnline = true;

                                if (ChatWindows.Any(f => ((FriendConnection)((FriendView)f.DataContext).Connection).UserName == friendConnection.UserName))
                                {
                                    var window = ChatWindows.Find(f => ((FriendConnection)((FriendView)f.DataContext).Connection).UserName == friendConnection.UserName);

                                    window.HandleMessage(p);

                                    if (window.IsVisible == false)
                                    {
                                        window.Show();

                                        try
                                        {
                                            var file = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System)).Parent.FullName + "\\Media\\notify.wav";
                                            var s = new System.Media.SoundPlayer(file);
                                            s.PlaySync();
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }

                                    window.Focus();
                                }
                                else
                                {
                                    var window = new FriendChatWindow();
                                    window.DataContext = GetFriendViewFromConnection(friendConnection);
                                    window.Show();
                                    window.HandleMessage(p);
                                    ChatWindows.Add(window);
                                }
                            }));

                        break;
                    }
                case FriendConnection.PRESENCE_ONLINE:
                    {
                        if (!friendConnection.IsOnline)
                        {
                            Notifications.NotificationsService.AddNotification("Friend Online",
                                friendConnection.UserName + " is now online!", 3000, true);

                            var view = GetFriendViewFromConnection(friendConnection);
                            view.Status = "Online - Idle";

                            friendConnection.IsOnline = true;
                        }
                        break;
                    }
                case FriendConnection.PRESENCE_OFFLINE:
                    {
                        if (friendConnection.IsOnline)
                        {
                            Notifications.NotificationsService.AddNotification("Friend Offline",
                                friendConnection.UserName + " is now offline!", 3000, true);

                            var view = GetFriendViewFromConnection(friendConnection);
                            view.Status = "Offline";

                            friendConnection.IsOnline = false;
                        }
                        break;
                    }
                case FriendConnection.AVATAR_SET:
                    {
                        var view = GetFriendViewFromConnection(friendConnection);
                        view.AvatarUrl = p;
                        break;
                    }
                default:
                    {
                        Hanasu.Services.Events.EventService.RaiseEvent(EventType.Friend_Data_Received, new FriendDataReceivedEventInfo()
                        {
                            Connection = friendConnection,
                            Data = p,
                            Type = type
                        });
                        break;
                    }
            }
        }

        public class FriendDataReceivedEventInfo : EventInfo
        {
            public FriendConnection Connection { get; set; }
            public string Data { get; set; }
            public string Type { get; set; }
        }

        public FriendView GetFriendViewFromConnection(FriendConnection conn)
        {
            return Instance.Friends.First(t => t.Connection == conn);
        }
        public FriendChatWindow GetChatWindow(FriendView friendView)
        {
            if (ChatWindows.Any(f => ((FriendConnection)((FriendView)f.DataContext).Connection).UserName == friendView.Connection.UserName))
            {
                var window = ChatWindows.Find(f => ((FriendConnection)((FriendView)f.DataContext).Connection).UserName == friendView.Connection.UserName);
                return window;
            }
            else
            {
                var window = new FriendChatWindow();
                window.DataContext = friendView;
                ChatWindows.Add(window);
                return window;
            }
        }
        public FriendChatWindow GetChatWindow(FriendConnection friendConnection)
        {
            return GetChatWindow(GetFriendViewFromConnection(friendConnection));
        }

        public void AddFriend(string username, string ip, int key, bool UseUDP = true, bool IfTcpIsHost = false)
        {
            var f = new FriendConnection(username, ip, key, UseUDP, IfTcpIsHost);
            f.Initiate(ip);
            Friends.Add(new FriendView(f));
            Instance.OnPropertyChanged("Friends");
        }
        public void DeleteFriend(FriendView fv)
        {
            Instance.Friends.Remove(fv);


            BindingOperations.ClearAllBindings((DependencyObject)fv);
        }

    }
    public delegate void EmptyDelegate();
}
