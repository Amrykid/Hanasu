using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hanasu.Services.Friends
{
    public class FriendsService : BaseINPC
    {
        public const int FriendsPort = 8562;

        static FriendsService()
        {
            if (Instance != null)
                Initialize();
        }
        public static void Initialize()
        {
            Instance = new FriendsService();
        }
        public static FriendsService Instance { get; private set; }

        private FriendsService()
        {
            Friends = new ObservableCollection<Friend>();

            mapping = new Mono.Nat.Mapping(Mono.Nat.Protocol.Udp, FriendsPort, FriendsPort);

            Task.Factory.StartNew(() =>
                {
                    Mono.Nat.NatUtility.DeviceFound += NatUtility_DeviceFound;
                    Mono.Nat.NatUtility.StartDiscovery();

                }).ContinueWith((tk) => tk.Dispose());

            App.Current.Exit += Current_Exit;
        }

        private Mono.Nat.Mapping mapping = null;
        private Mono.Nat.INatDevice Router = null;
        void NatUtility_DeviceFound(object sender, Mono.Nat.DeviceEventArgs e)
        {
            Mono.Nat.NatUtility.DeviceFound -= NatUtility_DeviceFound;

            Router = e.Device;
            Router.CreatePortMap(mapping);
        }

        void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            if (Router != null)
                Router.DeletePortMap(mapping);
            App.Current.Exit -= Current_Exit;
        }

        public ObservableCollection<Friend> Friends { get; private set; }
    }
}
