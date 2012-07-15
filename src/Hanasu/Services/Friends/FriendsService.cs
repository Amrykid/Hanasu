using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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

            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Station_Changed,
                e =>
                {
                    var e2 = (Hanasu.MainWindow.StationEventInfo)e;

                    foreach (FriendConnection f in Instance.Friends)
                        f.SendData("Now listening to " + e2.CurrentStation.Name, "STATUS_CHANGED");
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
                using (var fs = new FileStream(Instance.FriendsDBFile, FileMode.OpenOrCreate))
                {
                    IFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, Instance.Friends);
                    fs.Close();
                }
        }


        public static bool IsInitialized { get; private set; }
        public static FriendsService Instance { get; private set; }

        public string FriendsDBFile { get; private set; }
        public ObservableCollection<FriendConnection> Friends { get; set; }

        internal void HandleReceievedData(FriendConnection friendConnection, string type, string p)
        {
            switch (type.ToUpper())
            {
                case "STATUS_CHANGED":
                    {
                        friendConnection.Status = p;
                        Hanasu.Services.Notifications.NotificationsService.AddNotification(friendConnection.UserName + "'s Status",
                            p,
                            3000,
                            true,
                            Notifications.NotificationType.Now_Playing);
                    }
                    break;
            }
        }

        public void AddFriend(string username, string ip, int key)
        {
            var f = new FriendConnection(username, ip, key);
            Friends.Add(f);
            Instance.OnPropertyChanged("Friends");
        }
    }
}
