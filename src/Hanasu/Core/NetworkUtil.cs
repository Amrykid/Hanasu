using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Hanasu.Core
{
    public static class NetworkUtils
    {
        #region Internet Connection checking from http://stackoverflow.com/questions/843810/c-sharp-fastest-way-to-test-internet-connection
        [DllImport("wininet.dll", CharSet = CharSet.Auto)]
        private extern static bool InternetGetConnectedState(ref InternetConnectionState_e lpdwFlags, int dwReserved);
        //^ http://msdn.microsoft.com/en-us/library/aa384702%28VS.85%29.aspx
        [Flags]
        enum InternetConnectionState_e : int
        {
            INTERNET_CONNECTION_MODEM = 0x1,
            INTERNET_CONNECTION_LAN = 0x2,
            INTERNET_CONNECTION_PROXY = 0x4,
            INTERNET_RAS_INSTALLED = 0x10,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_CONFIGURED = 0x40
        }

        public static bool IsConnectedToInternet()
        {
            // In function for checking internet
            InternetConnectionState_e flags = 0;
            return InternetGetConnectedState(ref flags, 0);
        }
        #endregion
    }
}
