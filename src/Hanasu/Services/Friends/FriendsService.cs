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

            LoadFriends();

            foreach (var con in Instance.Friends)
                con.SetPresence(true);

            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Station_Changed,
                e =>
                {
                    var e2 = (Hanasu.MainWindow.StationEventInfo)e;

                    foreach (FriendConnection f in Instance.Friends)
                        f.SendStatusChange("Now listening to " + e2.CurrentStation.Name);
                });

            IsInitialized = true;
        }

        private static void LoadFriends()
        {
            if (System.IO.File.Exists(Instance.FriendsDBFile))
            {
                try
                {
                    using (var fs = new FileStream(Instance.FriendsDBFile, FileMode.OpenOrCreate))
                    {
                        IFormatter bf = new BinaryFormatter();
                        Instance.Friends = (ObservableCollection<FriendConnection>)bf.Deserialize(fs);
                        fs.Close();
                    }

                    foreach (FriendConnection f in Instance.Friends)
                        f.Initiate(f.IPAddress);

                    Instance.OnPropertyChanged("Friends");
                }
                catch (Exception)
                {
                    Instance.Friends = new ObservableCollection<FriendConnection>();
                }
            }
            else
            {
                Instance.Friends = new ObservableCollection<FriendConnection>();
            }
        }

        static void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            System.Windows.Application.Current.Exit -= Current_Exit;

            if (Instance.Friends.Count > 0)
            {
                foreach (var con in Instance.Friends)
                    con.SetPresence(false);

                using (var fs = new FileStream(Instance.FriendsDBFile, FileMode.OpenOrCreate))
                {
                    IFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, Instance.Friends);
                    fs.Close();
                }
            }
        }


        public static bool IsInitialized { get; private set; }
        public static FriendsService Instance { get; private set; }

        public string FriendsDBFile { get; private set; }
        public ObservableCollection<FriendConnection> Friends { get; set; }
        private List<FriendChatWindow> ChatWindows = new List<FriendChatWindow>();

        internal void HandleReceievedData(FriendConnection friendConnection, string type, string p)
        {
            switch (type.ToUpper())
            {
                case FriendConnection.STATUS_CHANGED:
                    {
                        friendConnection.Status = p;
                        friendConnection.IsOnline = true;

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

                                if (ChatWindows.Any(f => ((FriendConnection)f.DataContext).UserName == friendConnection.UserName))
                                {
                                    var window = ChatWindows.Find(f => ((FriendConnection)f.DataContext).UserName == friendConnection.UserName);

                                    window.HandleMessage(p);

                                    if (window.IsVisible == false)
                                        window.Show();

                                    window.Focus();
                                }
                                else
                                {
                                    var window = new FriendChatWindow();
                                    window.DataContext = friendConnection;
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

                            friendConnection.IsOnline = false;
                        }
                        break;
                    }
            }
        }

        public FriendChatWindow GetChatWindow(FriendConnection friendConnection)
        {
            if (ChatWindows.Any(f => ((FriendConnection)f.DataContext).UserName == friendConnection.UserName))
            {
                var window = ChatWindows.Find(f => ((FriendConnection)f.DataContext).UserName == friendConnection.UserName);
                return window;
            }
            else
            {
                var window = new FriendChatWindow();
                window.DataContext = friendConnection;
                ChatWindows.Add(window);
                return window;
            }
        }

        public void AddFriend(string username, string ip, int key)
        {
            var f = new FriendConnection(username, ip, key);
            Friends.Add(f);
            Instance.OnPropertyChanged("Friends");
        }
    }
    public delegate void EmptyDelegate();
}
