﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Media;
using System.ComponentModel.Composition;
using Hanasu.Core;
using WMPLib;
using System.Collections;
using System.Windows.Forms.Integration;
using System.Windows.Controls;

namespace Hanasu.Player.WMP
{
    [Export(typeof(IMediaPlayer))]
    public class WMPPlayer : IMediaPlayer
    {
        AxWMP host = null;
        AxWMPLib.AxWindowsMediaPlayer player = null;

        private Hashtable currentStationAttributes = new Hashtable();

        private string songName = null;
        private string stationName = null;

        private WindowsFormsHost wpfhost = null;

        private PlayerDetectedStationType stationType = PlayerDetectedStationType.None;

        public void Initialize()
        {
            return;
        }

        void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            switch ((WMPLib.WMPPlayState)e.newState)
            {
                case WMPPlayState.wmppsPlaying:
                    {
                        parseAttributes();

                        GlobalHanasuCore.OnStationMediaTypeDetected(this, IsVideo);


                        if (stationType == PlayerDetectedStationType.None && GlobalHanasuCore.CurrentStation.ServerType == Core.Stations.StationServerType.Auto)
                        {
                            try
                            {
                                if (Hanasu.Core.Stations.Shoutcast.ShoutcastService.GetIfShoutcastStation(currentStationAttributes))
                                {
                                    stationType = PlayerDetectedStationType.Shoutcast;
                                }
                                else
                                {
                                    stationType = PlayerDetectedStationType.Unknown;
                                }
                            }
                            catch (Exception)
                            {
                            }

                            Uri url = null;

                            try
                            {
                                url = new Uri(currentStationAttributes["SourceURL"].ToString());
                            }
                            catch (Exception)
                            {
                            }

                            GlobalHanasuCore.OnStationTypeDetected(this, new Tuple<PlayerDetectedStationType,Uri>(stationType,url));
                        }
                    }
                    break;
            }

        }

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
        {
            GlobalHanasuCore.OnStationConnectionTerminated(this);

            songName = null;
            stationName = null;
        }

        private List<string> radioMessages = new List<string>();
        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            //Differentating Song titles, station titles and radio messages is basic for now. Will be expanded to be equal to or better than Hanasu 1.0. It also differs based on the player selected.

            var name = player.currentMedia.name;

            var smallname = name.ToLower();
            var smallname2 = GlobalHanasuCore.CurrentStation.Name.ToLower();

            if (smallname.StartsWith(smallname2) && name != stationName)
            {
                GlobalHanasuCore.OnStationTitleDetected(this, name);
                stationName = name;
            }
            else
                if (name.Contains(" - ") && songName != name)
                {
                    GlobalHanasuCore.OnSongTitleDetected(this, name);
                    songName = name;
                }
                else
                {
                    if (!radioMessages.Contains(name) && name != songName)
                    {
                        //Radio message
                        GlobalHanasuCore.OnStationMessage(this, name);
                        radioMessages.Add(name);
                    }
                }

        }

        private void parseAttributes()
        {
            currentStationAttributes.Clear();
            for (int i = 0; i < player.currentMedia.attributeCount; i++)
            {
                var x = player.currentMedia.getAttributeName(i);
                var y = player.currentMedia.getItemInfo(x);

                currentStationAttributes.Add(x, y);
            }
        }

        private MediaElement mediaElement = null;
        private MediaType mType;
        private bool mediaElementPlaying = false;

        public void Play(Uri url, MediaType type = MediaType.Audio)
        {
            if (type != MediaType.Video)
            {
                radioMessages.Clear();

                if (player == null)
                {
                    host = new AxWMP();
                    player = host.axWindowsMediaPlayer1;
                    player.settings.autoStart = false;
                    player.MediaChange += player_MediaChange;
                    player.MediaError += player_MediaError;
                    player.PlayStateChange += player_PlayStateChange;
                    player.uiMode = "none";
                    player.stretchToFit = true;
                    //player.Dock = System.Windows.Forms.DockStyle.Fill;
                    player.enableContextMenu = false;
                    player.SendToBack();
                    host.SendToBack();

                    wpfhost = new WindowsFormsHost()
                    {
                        Child = host,
                    };
                }
                stationType = PlayerDetectedStationType.None;

                player.URL = url.ToString();
                player.Ctlcontrols.play();

                if (mediaElement != null)
                {
                    ShutdownMediaElement();
                }
            }
            else
            {
                if (mediaElement == null)
                {
                    mediaElement = new MediaElement();
                    mediaElement.LoadedBehavior = MediaState.Manual;
                    mediaElement.UnloadedBehavior = MediaState.Manual;
                    mediaElement.MediaOpened += (mediaElement_MediaOpened);
                    mediaElement.MediaFailed += (mediaElement_MediaFailed);
                    mediaElement.MediaEnded += (mediaElement_MediaEnded);
                    mediaElement.ScriptCommand += (mediaElement_ScriptCommand);
                }

                mediaElement.Source = url;
                mediaElement.Play();

                GlobalHanasuCore.OnStationMediaTypeDetected(this, true); //Hack to force Hanasu to grab the video control.

                if (player != null)
                    ShutdownWMP();
            }

            mType = type;
        }

        void mediaElement_ScriptCommand(object sender, System.Windows.MediaScriptCommandRoutedEventArgs e)
        {
#if DEBUG
            //System.Windows.MessageBox.Show("ScriptCommand Value: " + e.ParameterValue, "ScriptCommand Type: " + e.ParameterType);
#endif
        }

        private void ShutdownMediaElement()
        {
            if (mediaElement != null)
            {
                mediaElement.MediaOpened -= (mediaElement_MediaOpened);
                mediaElement.MediaFailed -= (mediaElement_MediaFailed);
                mediaElement.MediaEnded -= (mediaElement_MediaEnded);
                mediaElement.ScriptCommand -= (mediaElement_ScriptCommand);

                try
                {
                    mediaElement.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        void mediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            mediaElementPlaying = false;
        }

        void mediaElement_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            mediaElementPlaying = false;
        }

        void mediaElement_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            mediaElementPlaying = true;
        }

        public void Stop()
        {
            if (mType == MediaType.Video)
            {
                mediaElement.Stop();
            }
            else
            {
                if (player != null)
                    player.Ctlcontrols.stop();

                songName = null;
            }
        }


        public bool IsPlaying
        {
            get { return mType == MediaType.Video ? mediaElementPlaying : (player.playState == WMPLib.WMPPlayState.wmppsPlaying || player.playState == WMPLib.WMPPlayState.wmppsBuffering); }
        }


        public void Shutdown()
        {
            ShutdownWMP();
            ShutdownMediaElement();
        }

        private void ShutdownWMP()
        {
            if (player != null)
            {
                player.MediaChange -= player_MediaChange;
                player.MediaError -= player_MediaError;
                player.PlayStateChange -= player_PlayStateChange;
                //player.close();

                try
                {
                    player.Dispose();
                    host.Dispose();

                    player = null;
                    host = null;
                }
                catch (Exception)
                {
                }
            }
        }
        ~WMPPlayer()
        {
            Shutdown();
        }


        public bool Supports(string extension)
        {
            if (extension == null) return true;

            var supported = new string[] { ".asx", ".mp3", ".wav" }; //WMP supports M3U but I would like Hanasu to give people the choice to choose a stream.

            return supported.Contains(extension);
        }


        public int Volume
        {
            get
            {
                if (player != null)
                    return player.settings.volume;
                if (mediaElement != null)
                    return (int)mediaElement.Volume;

                return 0;
            }
            set
            {
                if (player != null)
                {
                    player.settings.volume = value;
                    return;
                }

                if (mediaElement != null)
                {
                    mediaElement.Volume = value;
                    return;
                }

            }
        }


        public bool IsVideo
        {
            get
            {
                if (mType == MediaType.Auto)
                {
                    if (currentStationAttributes.ContainsKey("MediaType"))
                        return currentStationAttributes["MediaType"].ToString().ToLower() == "video" || currentStationAttributes["MediaType"].ToString().ToLower() == "photo";
                }

                return mType == MediaType.Video;
            }
        }

        public object GetVideoControl()
        {
            return mediaElement;
        }

        public bool IsWMP12OrHigher()
        {
            if (player != null)
            {
                var x = Version.Parse(player.versionInfo);

                return x.Major >= 12;
            }
            else
            {
                return Environment.OSVersion.Version.Major > 6 || (Environment.OSVersion.Version.Major > 6 && Environment.OSVersion.Version.Minor >= 1); //Educated guess because Vista (6.0 can only have WMP 11) and 7 (6.1) can have WMP12.
            }

            return false;
        }


        public object GetPosition()
        {
            if (IsVideo)
                return mediaElement.Position;
            else
                return player.Ctlcontrols.currentPositionString;
        }


        public bool IsMuted
        {
            get
            {
                if (player != null)
                    return player.settings.mute;
                if (mediaElement != null)
                    return mediaElement.IsMuted;

                return false;
            }
            set
            {
                if (player != null)
                {
                    player.settings.mute = value;
                    return;
                }

                if (mediaElement != null)
                {
                    mediaElement.IsMuted = value;
                    return;
                }

            }
        }


        public Hashtable DataAttributes
        {
            get { return currentStationAttributes; }
        }
    }
}
