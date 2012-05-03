using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Hanasu.Services.IRC
{
    public class IRCService
    {
        private static AccelIRC.IRCClient irc = null;
        static IRCService()
        {
            /*if (irc == null)
                Initialize(); */
        }

        public static string Nick { get; set; }
        public static string User { get; set; }
        public static string Channel { get; set; }
        public static string Server { get; set; }
        public static int Port { get; set; }
        public static bool UseSSL { get; set; }

        public static void Initialize()
        {
            if (irc != null)
                return;

            Nick = "Amrykid[H]";
            User = "Amrykid";
            Channel = "##XAMPP";
            Server = "irc.freenode.net";
            Port = 6667;
            UseSSL = false;
            
            irc = new AccelIRC.IRCClient(Nick, User, Nick);
            irc.AutoReconnect = true;
            irc.Connect(Server, Port, UseSSL);
            irc.JoinChannel("##XAMPP");
           

            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Station_Changed,
                new Action<Events.EventInfo>(e =>
                {
                    var ei = (Hanasu.MainWindow.StationEventInfo)e;

                    irc.SendRaw("PRIVMSG " + Channel + " :ACTION " + "is listening to " + ei.CurrentStation.Name + "." + "");
                }));

            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Song_Liked,
                new Action<Events.EventInfo>(e =>
                {
                    var ei = (Hanasu.MainWindow.SongLikedEventInfo)e;

                    irc.SendRaw("PRIVMSG " + Channel + " :ACTION " + "liked [" + ei.CurrentSong.TrackTitle + " by " + ei.CurrentSong.Artist + "] on " + ei.CurrentStation.Name + "." + "");
                }));

        }
    }
}
