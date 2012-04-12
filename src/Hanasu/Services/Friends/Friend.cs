using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Hanasu.Core;

namespace Hanasu.Services.Friends
{
    public class Friend: BaseINPC
    {
        public string Name { get; set; }
        public Socket UDPSocket { get; set; }
        public string Station { get; set; }
    }
}
