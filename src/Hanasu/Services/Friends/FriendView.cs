using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;

namespace Hanasu.Services.Friends
{
    public class FriendView: BaseINPC
    {
        public FriendConnection Connection { get; private set; }
        internal FriendView(FriendConnection connection)
        {
            Connection = connection;
            OnPropertyChanged("AvatarUrl");
        }

        private string _status = null;
        public string Status { get { return _status; } 
            set { 
                _status = value; 
                OnPropertyChanged("Status"); } }
        public string UserName { get { return Connection.UserName; } }

        public string AvatarUrl { get { return Connection.AvatarUrl; } set { Connection.AvatarUrl = value; OnPropertyChanged("AvatarUrl"); } }
    }
}
