﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Hanasu.Services.Logging;
using Hanasu.Services.Song;
using Hanasu.Services.Stations;
using Hanasu.Windows;
using MahApps.Metro.Controls;
using System.Timers;
using WMPLib;
using System.Collections;
using Hanasu.Core;
using System.Diagnostics;
using System.IO;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Shell;
using System.Collections.Generic;
using Hanasu.Services.Friends;
using Hanasu.Services.Events;
using System.Net.NetworkInformation;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (App.Current == null) return;

                Hanasu.Services.Events.EventService.Initialize();
                Hanasu.Services.Facebook.FacebookService.Initialize();

                Hanasu.Services.Settings.SettingsService.Initialize();

                Hanasu.Services.Logging.LogService.Initialize();

                Hanasu.Services.Stations.StationsService.Initialize();
                Hanasu.Services.Stations.StationsService.Instance.StationFetchStarted += Instance_StationFetchStarted;
                Hanasu.Services.Stations.StationsService.Instance.StationFetchCompleted += Instance_StationFetchCompleted;

                Hanasu.Services.LikedSongs.LikedSongService.Initialize();

                Hanasu.Services.Schedule.ScheduleService.Initialize();

                Hanasu.Services.Friends.FriendsService.Initialize();

                HandleMediaKeyHooks();

                Hanasu.Services.Events.EventService.AttachHandler(Services.Events.EventType.Theme_Changed,
                    e =>
                    {
                        Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
                    });

                //Hanasu.Services.Friends.FriendsService.Initialize();

                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

                this.KeyUp += MainWindow_KeyUp;

                this.Loaded += MainWindow_Loaded;
                this.Unloaded += MainWindow_Unloaded;
            }
            else
            {
                //Is in the designer. Do nothing.
            }

            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);

        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (e.Exception.Message.Contains("Collection was modified")) return;

            ErrorWindow ew = new ErrorWindow();
            ew.DataContext = e.Exception;

#if DEBUG
            Debugger.Break();
#endif

            try
            {
                ew.Owner = this;

                ew.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            }
            catch (Exception)
            {
                ew.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            }

            ew.ShowDialog();
            ew.Close();
        }
        private static Hanasu.External.InterceptKeys.LowLevelKeyboardProc keyHookproc = null;
        private void HandleMediaKeyHooks()
        {
            keyHookproc = new External.InterceptKeys.LowLevelKeyboardProc(
                (int nCode, IntPtr wParam, IntPtr lParam) =>
                {
                    if (nCode >= 0 && wParam == (IntPtr)Hanasu.External.InterceptKeys.WM_KEYDOWN)
                    {
                        int vkCode = System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
                        var key = (System.Windows.Forms.Keys)vkCode;
                        //TODO: find a better way to do this. don't like mixing WPF and WinForms

                        switch (key)
                        {
                            case System.Windows.Forms.Keys.MediaPlayPause:
                                {
                                    if (NowPlayingGrid.Visibility == System.Windows.Visibility.Visible)
                                        player.Ctlcontrols.pause();
                                    else
                                        player.Ctlcontrols.play();

                                    return new IntPtr(1);
                                }
                            case System.Windows.Forms.Keys.MediaNextTrack:
                                {
                                    NextStation();
                                    return new IntPtr(1);
                                }
                            case System.Windows.Forms.Keys.MediaPreviousTrack:
                                {
                                    PreviousStation();
                                    return new IntPtr(1);
                                }
                            case System.Windows.Forms.Keys.MediaStop: player.Ctlcontrols.stop();
                                return new IntPtr(1);

                        }
                    }
                    return Hanasu.External.InterceptKeys.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                });

            Hanasu.External.InterceptKeys.SetHook(keyHookproc);
        }

        private void NextStation()
        {
            if (StationsListView.HasItems)
                if (StationsListView.Items.Count > (StationsListView.SelectedIndex - 1))
                {
                    StationsListView.SelectedIndex += 1;
                    StationsListView_MouseDoubleClick(StationsListView, null);
                }
        }
        private void PreviousStation()
        {
            if (StationsListView.HasItems)
                if ((StationsListView.SelectedIndex - 1) > -1)
                {
                    StationsListView.SelectedIndex -= 1;
                    StationsListView_MouseDoubleClick(StationsListView, null);
                }
        }

        void Instance_StationFetchCompleted(object sender, EventArgs e)
        {
            HideStationsAdorner();

            Hanasu.Services.Stations.StationsService.Instance.StationFetchStarted -= Instance_StationFetchStarted;
            Hanasu.Services.Stations.StationsService.Instance.StationFetchCompleted -= Instance_StationFetchCompleted;
        }

        void Instance_StationFetchStarted(object sender, EventArgs e)
        {
            ShowStationsAdorner();
        }

        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {

        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue();

            this.KeyUp -= MainWindow_KeyUp;

            if (IsWMPInitialized)
            {
                ShutdownWMPClose();
            }

            bufferTimer.Elapsed -= bufferTimer_Elapsed;
            bufferTimer.Dispose();

            this.Loaded -= MainWindow_Loaded;
            this.Unloaded -= MainWindow_Unloaded;
        }

        private AxWMPLib.AxWindowsMediaPlayer player = null;
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeWMPPControl();


            VolumeSlider.Value = Hanasu.Services.Settings.SettingsService.Instance.LastSetVolume;

            HandleWindowsTaskbarstuff();

            bufferTimer = new Timer();
            bufferTimer.Elapsed += bufferTimer_Elapsed;

            bufferTimer.Interval = 1000;

            currentStationAttributes = new Hashtable();

            attemptToConnectTimer = new Timer();
            attemptToConnectTimer.Elapsed += new ElapsedEventHandler(attemptToConnectTimer_Elapsed);
            attemptToConnectTimer.Interval = 20000; // 20 seconds

            this.tabControl1.SelectedIndex = 1;


            App.Current.MainWindow = this;
#if !DEBUG
            tabItem3.Visibility = System.Windows.Visibility.Hidden;
            ((App)App.Current).SplashScreen.Close(); //close the splash screen.
            App.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
#endif
        }

        #region WMP Stuff
        public bool IsWMP12OrHigher()
        {
            if (IsWMPInitialized)
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
        public bool IsWMPInitialized = false;
        private void InitializeWMPPControl()
        {
            if (IsWMPInitialized) return;

            //configure the wmp control
            windowsFormsHost1.Child = new Hanasu.Core.AxWMP();
            player = ((Hanasu.Core.AxWMP)windowsFormsHost1.Child).axWindowsMediaPlayer1;

            player.Dock = System.Windows.Forms.DockStyle.Fill;

            player.uiMode = "none";
            player.enableContextMenu = false;
            player.settings.autoStart = false;

            player.settings.volume = (int)VolumeSlider.Value;


            player.MediaChange += player_MediaChange;

            player.ScriptCommand += player_ScriptCommand;

            player.CurrentMediaItemAvailable += player_CurrentMediaItemAvailable;

            player.MarkerHit += player_MarkerHit;

            player.EndOfStream += player_EndOfStream;

            player.PlayStateChange += player_PlayStateChange;


            stationMediaWMP11orLowerTimer = new Timer();
            stationMediaWMP11orLowerTimer.Elapsed += new ElapsedEventHandler(stationMediaWMP11orLowerTimer_Elapsed);
            stationMediaWMP11orLowerTimer.Interval = (1000 * 60) * 3; //3 minutes

            songlengthPBTimer = new Timer();
            songlengthPBTimer.Elapsed += new ElapsedEventHandler(songlengthPBTimer_Elapsed);
            songlengthPBTimer.Interval = 1000;

            IsWMPInitialized = true;
        }

        void songlengthPBTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (currentSong != null)
            {
                Dispatcher.Invoke(new EmptyDelegate(() =>
                    {
                        SongLengthBar.Visibility = System.Windows.Visibility.Visible;
                        SongLengthBar.Maximum = currentSong.EstimatedSongLength.TotalSeconds;
                        SongLengthBar.Value++;

                        var bit = new TimeSpan(0, 0,
                            (int)SongLengthBar.Value);
                        SongLengthLabel.Content = (bit.Minutes + ":" + bit.Seconds + "/" + currentSong.EstimatedSongLength.Minutes + ":" + currentSong.EstimatedSongLength.Seconds).ToString();
                    }));
            }

        }

        void stationMediaWMP11orLowerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //WMP11 and lower cannot get the song title so we fallback to other methods of getting the song title.
            //This is also used if a station is set to use alternate title fetching.

            if (currentStation.Name == null) return;

            player_parseAttributes();

            Station_Wait_And_AlternateFetchSongTitle();
        }
        private void ShutdownWMPClose()
        {
            if (!IsWMPInitialized) return;

            player.PlayStateChange -= player_PlayStateChange;

            player.MediaChange -= player_MediaChange;

            player.MediaError -= player_MediaError;
            player.ScriptCommand -= player_ScriptCommand;
            player.MarkerHit -= player_MarkerHit;
            player.EndOfStream -= player_EndOfStream;
            player.CurrentMediaItemAvailable -= player_CurrentMediaItemAvailable;

            player.close();
            player.Dispose();

            ((Hanasu.Core.AxWMP)windowsFormsHost1.Child).Dispose();
        }
        #endregion

        void attemptToConnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var state = player.playState;

            if (player.playState != WMPPlayState.wmppsPlaying || player.playState != WMPPlayState.wmppsBuffering)
            {
                player.Ctlcontrols.stop();

                Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                    {
                        ShowUnableToConnect();
                    }));


                Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue();
            }

            attemptToConnectTimer.Stop();
        }

        void player_CurrentMediaItemAvailable(object sender, AxWMPLib._WMPOCXEvents_CurrentMediaItemAvailableEvent e)
        {
            if (currentStation.StationType == StationType.Radio)
            {
                try
                {
                    IWMPMedia3 media = (IWMPMedia3)player.mediaCollection.getByName(e.bstrItemName);
                    IWMPMetadataPicture pic = (IWMPMetadataPicture)media.getItemInfoByType("WM/Picture", "", 0);
                }
                catch (Exception)
                {
                }
            }
        }

        void player_EndOfStream(object sender, AxWMPLib._WMPOCXEvents_EndOfStreamEvent e)
        {

        }

        void player_MarkerHit(object sender, AxWMPLib._WMPOCXEvents_MarkerHitEvent e)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        void player_ScriptCommand(object sender, AxWMPLib._WMPOCXEvents_ScriptCommandEvent e)
        {
#if DEBUG
            throw new NotImplementedException();
#endif
        }

        void bufferTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                {
                    BufferingSP.Visibility = System.Windows.Visibility.Visible;
                    BufferPB.Value = player.network.bufferingProgress;

                    if (this.TaskbarItemInfo != null)
                    {
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        this.TaskbarItemInfo.ProgressValue = BufferPB.Value / 100;
                    }
                }));
        }

        #region Windows 7+ Taskbar stuff
        private bool IsWindows7OrHigher()
        {
            return ((Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1) /* if windows 7 */
                || Environment.OSVersion.Version.Major > 6); /* if windows 8, etc */
        }
        private JumpList HanasuJumpList = null;
        private void HandleWindowsTaskbarstuff()
        {
            //http://joshsmithonwpf.wordpress.com/2007/03/09/how-to-programmatically-click-a-button/

            //check if W7 or higher
            if (IsWindows7OrHigher())
            {
                HanasuJumpList = new JumpList();

                this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                this.TaskbarItemInfo.ThumbButtonInfos = new System.Windows.Shell.ThumbButtonInfoCollection();

                HanasuJumpList.ShowRecentCategory = true;
                JumpList.SetJumpList(Application.Current, HanasuJumpList);

                UriToBitmapImageCachedConverter uri = new UriToBitmapImageCachedConverter();

                AddThumbButton("Play", ButtonPlayClickCommand, playBtn, (ImageSource)uri.Convert("pack://application:,,,/Hanasu;component/Resources/play.png", null, new int[] { 400, 400 }, null), (e, s) => playBtn_Click(null, null));
                AddThumbButton("Pause/Stop", ButtonPauseClickCommand, pauseBtn, (ImageSource)uri.Convert("pack://application:,,,/Hanasu;component/Resources/pause.png", null, new int[] { 400, 400 }, null), (e, s) => pauseBtn_Click(null, null));

            }
        }
        private void AddThumbButton(string desc, RoutedCommand cmd, IInputElement CommandTarget, ImageSource imagesource, Action<object, ExecutedRoutedEventArgs> act)
        {
            CommandBinding commandBinding = new CommandBinding(cmd, (object sender2, ExecutedRoutedEventArgs e2) =>
            {
                act(sender2, e2);
            });
            this.CommandBindings.Add(commandBinding);

            this.TaskbarItemInfo.ThumbButtonInfos.Add(new System.Windows.Shell.ThumbButtonInfo()
            {
                Description = desc,
                CommandTarget = CommandTarget,
                Command = cmd,
                ImageSource = imagesource,
            });
        }

        public static RoutedCommand ButtonPlayClickCommand = new RoutedCommand();
        public static RoutedCommand ButtonPauseClickCommand = new RoutedCommand();
        #endregion


        private Timer bufferTimer = null;

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
        {
            attemptToConnectTimer.Stop();
            ShowUnableToConnect();
        }

        private void ShowUnableToConnect()
        {
            if (!unableToConnectNotificationShown)
            {
                Hanasu.Services.Notifications.NotificationsService.AddNotification(
                    "Unable to connect to station.",
                    "Hanasu was unable to connect to " + currentStation.Name + ".", 4000, false, Services.Notifications.NotificationType.Error);

                unableToConnectNotificationShown = true;

                HideStationsAdorner(); //On error rename the stations listview.
            }
        }

        private bool unableToConnectNotificationShown = false;
        private string lastMediaTxt = null; //prevents the below event from constantly queueing the same song title.
        private string lastSongTxt = null;
        private SongData currentSong;
        internal Hashtable currentStationAttributes { get; set; }
        private List<string> songMessages = new List<string>();
        private Timer songlengthPBTimer = new Timer();
        /// <summary>
        /// For alternate title fetching process (mostly for use with WMP11 and lower. Also used for stations set to use alternate title fetching).
        /// </summary>
        private Timer stationMediaWMP11orLowerTimer = null;
        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            try
            {
                player_parseAttributes();

                if (IsWMP12OrHigher() && currentStation.UseAlternateSongTitleFetching == false)
                {
                    #region WMP12 Handling
                    if (lastMediaTxt != player.currentMedia.name)
                    {
                        var name = player.currentMedia.name;

                        if ((Hanasu.Services.Song.SongService.IsSongTitle(name, currentStation)))
                        {
                            #region Handle Song Title
                            MoreInfoBtn.Visibility = System.Windows.Visibility.Hidden;
                            AddRawSongToLikedBtn.IsEnabled = true;

                            lastMediaTxt = name;
                            lastSongTxt = name;

                            SongDataLbl.Text = name;

                            SongIsLiked = false;

                            //LikeBtnInfo.IsEnabled = true;

                            songMessages.Clear();




                            //song changed. maybe a couple of seconds late.

                            //StopSongTimeMeasure();

                            if (currentStation.StationType == StationType.Radio)
                            {
                                //Dispatcher.Invoke(new EmptyDelegate(() =>
                                //                   {
                                //                       songlengthPBTimer.Stop();
                                //                       SongLengthBar.Visibility = System.Windows.Visibility.Collapsed;
                                //                   }));

                                if (Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name))
                                {
                                    object imgval = null;
                                    bool songDetectionFull = false;

                                    try
                                    {
                                        var songdat = Hanasu.Services.LikedSongs.LikedSongService.Instance.GetSongFromString(name);
                                        imgval = songdat.AlbumCoverData != null ? (object)songdat.AlbumCoverData : (songdat.AlbumCoverUri != null ? songdat.AlbumCoverUri.ToString() : null);

                                        currentSong = songdat;
                                        songDetectionFull = true;



                                    }
                                    catch (Exception) { }
                                    finally
                                    {
                                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                                        name, 4000, false, Services.Notifications.NotificationType.Now_Playing, null, imgval);
                                    }

                                    if (songDetectionFull)
                                    {
                                        if (currentSong != null)
                                        {
                                            //StartSongLengthDisplay();
                                        }
                                    }


                                }
                                else
                                {
                                    Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                                        name, 4000, false, Services.Notifications.NotificationType.Now_Playing);
                                }

                                #region Handle Radio stuff
                                if (!Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name))
                                {
                                    if (Hanasu.Services.Settings.SettingsService.Instance.AutomaticallyFetchSongData)
                                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                                            {
                                                var n = name;
                                                var stat = currentStation;

                                                System.Threading.Thread.Sleep(1000 * 20); //wait 20 seconds. if the user is still listening to the station, pull the lyrics. this creates less stress/http request to the lyrics site.

                                                if (stat != currentStation)
                                                    return;

                                                if (!Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name)) //Song could have been manually liked in the 20 seconds.
                                                {
                                                    Uri lyricsUrl = null;

                                                    if (Hanasu.Services.Song.SongService.IsSongAvailable(name, currentStation, out lyricsUrl))
                                                    {
                                                        if ((bool)Dispatcher.Invoke(
                                                               new Hanasu.Services.Notifications.NotificationsService.EmptyReturnDelegate(() =>
                                                               {
                                                                   return (n != SongDataLbl.Text);

                                                               })))
                                                            return;

                                                        if (Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(name))
                                                            return;

                                                        Hanasu.Services.Notifications.NotificationsService.AddNotification(name.Substring(0, name.Length / 2) + "..." + " - Song info found",
                                                        "Lyrics and other data found for this song. Click here to view.", 4000, false, Services.Notifications.NotificationType.Music_Data, (t) => MoreInfoBtn_Click(MoreInfoBtn, null));

                                                        Dispatcher.Invoke(
                                                            new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                                                {
                                                                    MoreInfoBtn.Visibility = System.Windows.Visibility.Visible;
                                                                    LikeBtnInfo.IsEnabled = true;

                                                                    currentSong = Hanasu.Services.Song.SongService.GetSongData(name, currentStation);

                                                                    MoreInfoBtn.DataContext = currentSong;
                                                                }));
                                                    }
                                                }
                                            }).ContinueWith((tk) => tk.Dispose());
                                }
                                else
                                {
                                    AddRawSongToLikedBtn.IsEnabled = false;

                                    SongData dat = new SongData();
                                    try
                                    {
                                        dat = Hanasu.Services.LikedSongs.LikedSongService.Instance.GetSongFromString(name);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    finally
                                    {
                                        if (dat.TrackTitle != null)
                                        {
                                            Hanasu.Services.Notifications.NotificationsService.AddNotification(name.Substring(0, name.Length / 2) + "..." + " - Liked Song Detected",
                                                "To view lyrics/album data, click here.", 4000, false, Services.Notifications.NotificationType.Music_Data, (t) =>
                                                {
                                                    /*if (this.WindowState == System.Windows.WindowState.Minimized)
                                                        this.WindowState = System.Windows.WindowState.Normal;*/

                                                    /*tabControl1.SelectedIndex = 2;

                                                    LikedSongsListView.SelectedItem = dat;

                                                    LikedSongsListView_MouseDoubleClick(LikedSongsListView, null); */
                                                    Hanasu.Windows.SongInfoWindow siw = new Windows.SongInfoWindow();
                                                    siw.DataContext = dat;

                                                    siw.Owner = this;

                                                    siw.ShowDialog();
                                                    siw.Close();
                                                });

                                            //if (currentSong.EstimatedSongLength == default(TimeSpan))
                                            //    StartSongTimeMeasure();
                                            //else
                                            //    StartSongLengthDisplay();
                                        }
                                        else
                                        {


                                            Hanasu.Services.Notifications.NotificationsService.AddNotification(name.Substring(0, name.Length / 2) + "..." + " - Possible Liked Song Detected",
                                                "Unable to retrieve information.", 4000, false, Services.Notifications.NotificationType.Music_Data);
                                        }
                                    }

                                }
                                #endregion
                            }
                            else
                            {
                                Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                                name, 4000, false, Services.Notifications.NotificationType.Now_Playing);
                            }
                            #endregion
                        }
                        else
                        {
                            Dispatcher.Invoke(
                                                new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                                {
                                                    MoreInfoBtn.Visibility = System.Windows.Visibility.Hidden;
                                                }));


                            //since its not a verified song, might as well display it as a radio message instead of 'Now Playing'.

                            currentSong = new SongData();

                            SongIsLiked = false;

                            LikeBtnInfo.IsEnabled = false;

                            lastMediaTxt = name;

                            if (currentStation.StationType == StationType.Radio)
                                if (currentStationAttributes.ContainsKey("WM/AlbumTitle"))
                                {
                                    SongDataLbl.Text = (string)currentStationAttributes["WM/AlbumTitle"];
                                    lastMediaTxt = (string)currentStationAttributes["WM/AlbumTitle"];

                                    //WMP12_Wait_And_AlternateFetchSongTitle();

                                    if (!Hanasu.Services.Song.SongService.IsSongTitle(lastMediaTxt, currentStation))
                                        AddRawSongToLikedBtn.IsEnabled = false;
                                }
                                else if (currentStationAttributes.ContainsKey("Title"))
                                {
                                    SongDataLbl.Text = (string)currentStationAttributes["Title"];
                                    lastMediaTxt = (string)currentStationAttributes["Title"];

                                    //WMP12_Wait_And_AlternateFetchSongTitle();

                                    if (!Hanasu.Services.Song.SongService.IsSongTitle(lastMediaTxt, currentStation))
                                        AddRawSongToLikedBtn.IsEnabled = false;
                                }
                                else
                                {
                                    SongDataLbl.Text = "Not Available";
                                    lastMediaTxt = "Not Available";
                                    AddRawSongToLikedBtn.IsEnabled = false;
                                }
                            else if (currentStation.StationType == StationType.TV)
                                SongDataLbl.Text = currentStation.Name;


                            if (songMessages.Contains(name) == false)
                            {
                                Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - " + (currentStation.StationType == StationType.Radio ? "Radio Message" : "TV Message"),
                                    name, 4000, false, Services.Notifications.NotificationType.Information);
                                songMessages.Add(name);
                            }
                        }
                    }
                    else
                    {
                        if (lastMediaTxt.ToLower().Contains(currentStation.Name.ToLower()) && lastSongTxt != lastMediaTxt)
                        {
                            lastMediaTxt = null;
                        }
                    }
                    #endregion
                }
                else
                {
                    //WMP 11 and lower cannot get the song title from streams so display anything we get as radio/tv messages.

                    var name = player.currentMedia.name;
                    if (songMessages.Contains(name) == false)
                    {
                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - " + (currentStation.StationType == StationType.Radio ? "Radio Message" : "TV Message"),
                            name, 4000, false, Services.Notifications.NotificationType.Information);
                        songMessages.Add(name);
                    }
                    return;
                }
            }
            catch (Exception)
            {
            }

        }

        //private void StartSongLengthDisplay()
        //{
        //    if (currentSong.EstimatedSongLength == default(TimeSpan))
        //    {
        //        StartSongTimeMeasure();
        //    }
        //    else
        //    {
        //        if (Hanasu.Services.Stations.StationsService.GetIfShoutcastStation(currentStationAttributes))
        //        {
        //            var time = Hanasu.Services.Stations.StationsService.GetShoutcastStationCurrentSongStartTime(currentStation, currentStationAttributes);

        //            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(
        //                             TimeZoneInfo.ConvertTimeToUtc(time, currentStation.TimeZoneInfo), TimeZoneInfo.Local);

        //            Dispatcher.Invoke(new EmptyDelegate(() =>
        //            {
        //                var faketime = new TimeSpan(0, time.Minute, time.Second);
        //                var diff = DateTime.Now - localTime;
        //                SongLengthBar.Value = diff.TotalSeconds;
        //                songlengthPBTimer.Start();
        //            }));
        //        }
        //    }
        //}

        //private SongData newCurrentSong;
        //private void StopSongTimeMeasure()
        //{
        //    try
        //    {
        //        if (currentSong.TrackTitle != null && Hanasu.Services.LikedSongs.LikedSongService.Instance.IsLiked(currentSong) && newCurrentSong._timeStart != default(DateTime))
        //        {
        //            //Grab the estimated song length

        //            var time = GetSongStartTime();

        //            newCurrentSong.EstimatedSongLength = time - newCurrentSong._timeStart;

        //            Hanasu.Services.LikedSongs.LikedSongService.Instance.LikedSongs[Hanasu.Services.LikedSongs.LikedSongService.Instance.LikedSongs.IndexOf(currentSong)] = newCurrentSong;
        //            Hanasu.Services.LikedSongs.LikedSongService.SaveLikedSongsDB();

        //            currentSong = new SongData();
        //            newCurrentSong = new SongData();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return;
        //    }
        //}

        //private void StartSongTimeMeasure()
        //{
        //    if (currentSong.TrackTitle != null && Hanasu.Services.LikedSongs.LikedSongService.Instance.IsLiked(currentSong))
        //    {
        //        if (currentSong.EstimatedSongLength.Minutes == 0 && currentSong._timeStart.Minute == 0)
        //        {
        //            try
        //            {
        //                var time = GetSongStartTime();

        //                //songlengthStopWatch.Start();

        //                newCurrentSong = currentSong;
        //                newCurrentSong.EstimatedSongLength = new TimeSpan();
        //                newCurrentSong._timeStart = time;

        //                Hanasu.Services.Notifications.NotificationsService.AddNotification(currentSong.TrackTitle,
        //                    "The duration of this song will be recorded for time estimation.", 3000, true);
        //            }
        //            catch (Exception)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        private DateTime GetSongStartTime()
        {
            if ((bool)Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyReturnDelegate(() => StationHistoryBtn.IsEnabled))) //True if the station was detected as a shoutcast station earlier.
            {
                return Hanasu.Services.Stations.StationsService.GetShoutcastStationCurrentSongStartTime(currentStation, currentStationAttributes);
            }

            throw new Exception();
        }

        private volatile bool Station_Wait_And_AlternateFetchSongTitle_fetching = false;
        private void Station_Wait_And_AlternateFetchSongTitle()
        {
            if (Station_Wait_And_AlternateFetchSongTitle_fetching) return;

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(t =>
            {
                Station_Wait_And_AlternateFetchSongTitle_fetching = true;

                var station = currentStation;

                System.Threading.Thread.Sleep(5000);

                if (station != currentStation) return;

                if ((bool)Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyReturnDelegate(() => StationHistoryBtn.IsEnabled))) //True if the station was detected as a shoutcast station earlier.
                {

                    try
                    {
                        var songdata = Hanasu.Services.Stations.StationsService.GetShoutcastStationCurrentSong(currentStation, currentStationAttributes);
                        currentSong = songdata;

                        Dispatcher.Invoke(new EmptyDelegate(() =>
                        {

                            var songStr = songdata.ToSongString();

                            if (SongDataLbl.Text != songStr)
                            {

                                lastMediaTxt = songStr;

                                SongDataLbl.Text = songdata.ToSongString();

                                Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                                            songdata.ToSongString(), 4000, false, Services.Notifications.NotificationType.Now_Playing);

                                AddRawSongToLikedBtn.IsEnabled = true;
                            }



                        }));
                    }
                    catch (Exception)
                    {
                        AddRawSongToLikedBtn.IsEnabled = false;
                    }

                }
                else
                {
                    Dispatcher.Invoke(new EmptyDelegate(() =>
                    {
                        AddRawSongToLikedBtn.IsEnabled = false;
                    }));
                }


                Station_Wait_And_AlternateFetchSongTitle_fetching = false;
            }));
        }

        void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //handle the wmp's status

            WMPLib.WMPPlayState state = (WMPLib.WMPPlayState)e.newState;

            LogService.Instance.WriteLog(player,
    "Play state changed: " + Enum.GetName(typeof(WMPLib.WMPPlayState), state));

            switch (state)
            {
                case WMPLib.WMPPlayState.wmppsTransitioning:
                    // Connecting, disable the stations listview.
                    ShowStationsAdorner();

                    break;
                case WMPLib.WMPPlayState.wmppsBuffering: BufferingSP.Visibility = System.Windows.Visibility.Visible;
                    BufferPB.Value = player.network.bufferingProgress;
                    bufferTimer.Start();
                    attemptToConnectTimer.Stop();
                    break;
                case WMPPlayState.wmppsWaiting:
                    {
                        attemptToConnectTimer.Stop();
                        if (this.TaskbarItemInfo != null)
                            this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                    }
                    break;
                case WMPLib.WMPPlayState.wmppsPlaying: NowPlayingGrid.Visibility = System.Windows.Visibility.Visible;
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;
                    bufferTimer.Stop();
                    attemptToConnectTimer.Stop();
                    playBtn.IsEnabled = false;
                    pauseBtn.IsEnabled = true;

                    player_parseAttributes();

                    if (!IsWMP12OrHigher() || currentStation.UseAlternateSongTitleFetching)
                    {
                        Station_Wait_And_AlternateFetchSongTitle();

                        stationMediaWMP11orLowerTimer.Start();
                    }

                    if (lastSongTxt == null)
                        AddRawSongToLikedBtn.IsEnabled = false;
                    else
                        AddRawSongToLikedBtn.IsEnabled = Hanasu.Services.Song.SongService.IsSongTitle(lastSongTxt, currentStation);

                    if (this.TaskbarItemInfo != null)
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;

                    unableToConnectNotificationShown = false;

                    player.settings.mute = isMuted;

                    if (!player.settings.mute)
                    {
                        VolumeSlider.IsEnabled = true;
                    }
                    VolumeMuteBtn.IsEnabled = true;

                    HideStationsAdorner(); //Playing, hide the adorner and rename the listview.

                    var laststation = currentStation;
                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(t =>
                        {
                            var b = Hanasu.Services.Stations.StationsService.GetIfShoutcastStation(currentStationAttributes);
                            if (player.playState == WMPPlayState.wmppsPlaying && currentStation == laststation)
                            {
                                Dispatcher.Invoke(new EmptyDelegate(() =>
                                    {
                                        StationHistoryBtn.IsEnabled = b;
                                    }));
                            }
                        }));

                    Hanasu.Services.Events.EventService.RaiseEventAsync(Services.Events.EventType.Station_Changed
                            , new StationEventInfo()
                            {
                                CurrentStation = currentStation
                            });

                    if (IsWindows7OrHigher())
                    {
                        if (HanasuJumpList.JumpItems.Find(t => ((JumpTask)t).Title == currentStation.Name) == null)
                        {
                            HanasuJumpList.JumpItems.Add(
                                new JumpTask()
                                {
                                    ApplicationPath = Application.ResourceAssembly.Location,
                                    Description = "Play " + currentStation.Name,
                                    Title = currentStation.Name,
                                    CustomCategory = "Recent",
                                    Arguments = "/play_station " + currentStation.Name,
                                });
                            HanasuJumpList.Apply();
                        }
                    }

                    break;
                case WMPLib.WMPPlayState.wmppsReady:
                case WMPLib.WMPPlayState.wmppsPaused:
                case WMPLib.WMPPlayState.wmppsStopped: NowPlayingGrid.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = true;
                    pauseBtn.IsEnabled = false;
                    AddRawSongToLikedBtn.IsEnabled = false;
                    StationHistoryBtn.IsEnabled = false;



                    if (player.playState == WMPPlayState.wmppsStopped) //if its paused.. keep playing.
                    {
                        currentSong = new SongData();
                        songlengthPBTimer.Stop();
                        SongLengthBar.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    bufferTimer.Stop();
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;

                    unableToConnectNotificationShown = false;

                    VolumeSlider.IsEnabled = false;
                    VolumeMuteBtn.IsEnabled = false;


                    if (stationMediaWMP11orLowerTimer != null)
                    {
                        stationMediaWMP11orLowerTimer.Stop();
                    }

                    //currentStation = new Station();

                    Hanasu.Services.Events.EventService.RaiseEvent(Services.Events.EventType.Station_Player_Idle,
                        EventInfo.Empty);

                    if (Hanasu.Services.Stations.StationsService.Instance.Status != StationsServiceStatus.Polling)
                        HideStationsAdorner();

                    if (this.TaskbarItemInfo != null)
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    break;
            }
        }

        private void player_parseAttributes()
        {
            currentStationAttributes.Clear();
            for (int i = 0; i < player.currentMedia.attributeCount; i++)
            {
                var x = player.currentMedia.getAttributeName(i);
                var y = player.currentMedia.getItemInfo(x);

                currentStationAttributes.Add(x, y);
            }

            OnPropertyChanged("currentStationAttributes");
        }

        private Station currentStation;

        private void StationsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsWMPInitialized)
                InitializeWMPPControl();

            //get the selected station and play it

            var station = (Station)StationsListView.SelectedItem;

            if (station.Name == null)
                return;

            if (NetworkUtils.IsConnectedToInternet() == false)
            {
                MessageBox.Show("No internet connection detected!");
                return;
            }

            if (station.LocalStationFile == null && !station.Cacheable)
            {

                try
                {
                    var status = new Ping().Send(station.DataSource.Host).Status;
                    if (status != IPStatus.Success)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {

                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Station is down.",
                            "The station you are trying to connect to is experiencing network issues. Please try again later.", 5000, true, Services.Notifications.NotificationType.Information);
                    return;
                }
            }

            Hanasu.Services.Stations.StationsService.CheckAndDownloadCacheableStation(ref station);



            Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue(); //Get rid of messages from the last station, if any.

            if (player.playState == WMPPlayState.wmppsPlaying)
                player.close();

            if (Hanasu.Services.Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(station.DataSource, station.ExplicitExtension))
            {
                var d = station.Cacheable && station.StationType != StationType.TV && station.LocalStationFile != null && File.Exists(station.LocalStationFile.LocalPath) ? station.LocalStationFile : station.DataSource;
                //Hanasu.Services.Preprocessor.PreprocessorService.Process(ref d);

                var pro = Hanasu.Services.Preprocessor.PreprocessorService.GetProcessor(d, station.ExplicitExtension);

                //Check if its a multi-stream station.
                if (pro.GetType().BaseType == typeof(Hanasu.Services.Preprocessor.MultiStreamPreprocessor))
                {
                    var p = (Hanasu.Services.Preprocessor.MultiStreamPreprocessor)pro;

                    var entries = p.Parse(d);

                    if (entries.Length == 0)
                    {
                        return;
                    }
                    else if (entries.Length == 1)
                    {
                        d = new Uri(entries[0].File);
                    }
                    else
                    {
                        //show a GUI here for choosing.
                        MultiStreamChooseWindow pls = new MultiStreamChooseWindow();
                        pls.DataContext = entries;
                        pls.Owner = this;

                        if (pls.ShowDialog() == true)
                        {
                            Hanasu.Services.Preprocessor.IMultiStreamEntry en = (Hanasu.Services.Preprocessor.IMultiStreamEntry)pls.listBox1.SelectedItem;

                            d = new Uri(en.File);
                            pls.Close();
                        }
                        else
                        {
                            pls.Close();
                            return;
                        }
                    }
                }

                Hanasu.Services.Preprocessor.PreprocessorService.Process(ref d);

                player.URL = d.ToString();
            }
            else
            {
                if (station.Cacheable && station.StationType != StationType.TV && station.LocalStationFile != null && File.Exists(station.LocalStationFile.LocalPath))
                    player.URL = station.LocalStationFile.ToString();
                else
                    player.URL = station.DataSource.ToString();
            }

            LogService.Instance.WriteLog(typeof(MainWindow),
    "Playing station: " + station.Name);

            if (station.StationType == StationType.TV)
            {
                player.network.bufferingTime = 10000; // 10 seconds ahead
                SongDataLbl.Text = station.Name;

            }
            else
            {
                player.network.bufferingTime = 2000; // 2 seconds
                SongDataLbl.Text = "Not Available";
            }

            lastSongTxt = null;
            lastMediaTxt = null;

            currentStationAttributes.Clear();

            player.Ctlcontrols.play();

            attemptToConnectTimer.Start();

            currentStation = station;

            //Hanasu.Services.Events.EventService.RaiseEventAsync(Services.Events.EventType.Station_Changed
            //    , new StationEventInfo()
            //    {
            //        CurrentStation = currentStation
            //    });

            if (station.StationType == StationType.TV)
                tabControl1.SelectedIndex = 0;
        }

        private Timer attemptToConnectTimer = null;


        public class StationEventInfo : Hanasu.Services.Events.EventInfo
        {
            public Station CurrentStation { get; internal set; }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsWMPInitialized)
                player.settings.volume = (int)VolumeSlider.Value;

            Hanasu.Services.Settings.SettingsService.Instance.LastSetVolume = (int)VolumeSlider.Value;
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsWMPInitialized)
                player.Ctlcontrols.play();
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentStation.Name != null)
                if (currentStation.StationType == StationType.Radio)
                    player.Ctlcontrols.pause();
                else if (currentStation.StationType == StationType.TV)
                    player.Ctlcontrols.stop();
        }

        private void ShowStationsAdorner()
        {
            LogService.Instance.WriteLog(typeof(MainWindow),
    "Station adorner shown.");

            StationsListAdorner.IsAdornerVisible = true;
            StationsListView.IsEnabled = false;
            this.IsEnabled = false;

            if (this.TaskbarItemInfo != null)
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }
        private void HideStationsAdorner()
        {
            LogService.Instance.WriteLog(typeof(MainWindow),
    "Station adorner hid.");

            StationsListAdorner.IsAdornerVisible = false;
            StationsListView.IsEnabled = true;

            this.IsEnabled = true;

            if (this.TaskbarItemInfo != null)
                this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
        }

        private void LogListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (LogListView.ItemsSource == null) return;

            if (LogListView.Items.Count > 0)
            {
                LogListView.SelectedIndex = LogListView.Items.Count - 1;
                LogListView.ScrollIntoView(LogListView.SelectedItem);
            }
        }

        private void MoreInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            Hanasu.Windows.SongInfoWindow siw = new Windows.SongInfoWindow();
            siw.DataContext = b.DataContext;

            siw.Owner = this;

            siw.ShowDialog();
            siw.Close();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Owner = this;
            sw.ShowDialog();
            sw.Close();
        }

        private void ffBtn_Click(object sender, RoutedEventArgs e)
        {
            NextStation();
        }

        private void revBtn_Click(object sender, RoutedEventArgs e)
        {
            PreviousStation();
        }

        private bool isMuted = false;
        private void VolumeMuteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWMPInitialized) return;

            if (player.settings.mute)
            {
                this.VolumeMuteBtnVisualBrush.Visual = (Visual)this.Resources["appbar_sound_3"];

                VolumeSlider.IsEnabled = true;

                player.settings.mute = false;

                player.settings.volume = (int)VolumeSlider.Value;

                isMuted = false;
            }
            else
            {
                this.VolumeMuteBtnVisualBrush.Visual = (Visual)this.Resources["appbar_sound_mute"];

                VolumeSlider.IsEnabled = false;

                player.settings.mute = true;

                isMuted = true;

            }
        }

        public bool SongIsLiked = false;
        private void LikeBtnInfo_Click(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Events.EventService.RaiseEvent(Services.Events.EventType.Song_Liked,
                new SongLikedEventInfo()
                {
                    CurrentSong = currentSong,
                    CurrentStation = currentStation
                });
            SongIsLiked = true;

            //StartSongTimeMeasure();

            LikeBtnInfo.IsEnabled = false;
            AddRawSongToLikedBtn.IsEnabled = false;

        }
        public class SongLikedEventInfo : StationEventInfo
        {
            public SongData CurrentSong { get; set; }
        }

        private void StationAttributesBtn_Click(object sender, RoutedEventArgs e)
        {
            StationAttributesWindow saw = new StationAttributesWindow();
            saw.DataContext = this;
            saw.Owner = this;
            saw.ShowDialog();
            saw.Close();
        }

        #region INotifyPropertyChanged / BaseINPC
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private void StationsListViewViewHomepageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (StationsListView.SelectedItem != null)
                Process.Start(((Station)StationsListView.SelectedItem).Homepage.ToString());
        }

        private void StationsListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Prevents double clicking of headers so they don't connect to stations.

            UIElement control = (UIElement)e.MouseDevice.DirectlyOver;

            if (control.TryFindParent<GridViewColumnHeader>() != null)
            {
                //is a header.
                e.Handled = true;

                HandleSort(sender, e, control);
            }
        }

        private void HandleSort(object sender, MouseButtonEventArgs e, UIElement control)
        {
            // Get the default view from the listview  

            var header = control.TryFindParent<GridViewColumnHeader>();

            if (header.Column == null) return;

            ICollectionView view = null;
            string property = null;

            if (sender == StationsListView)
            {
                view = CollectionViewSource.GetDefaultView(StationsListView.ItemsSource);


                switch (header.Column.Header.ToString())
                {
                    case "Station": property = "Name";
                        break;
                    case "Format": property = "Format";
                        break;
                    case "*": property = "StationType";
                        break;
                    default:
                        if (header.Column.Header.ToString().StartsWith("Station Language"))
                            property = "Language";
                        break;
                }
            }
            else if (sender == LikedSongsListView)
            {
                view = CollectionViewSource.GetDefaultView(LikedSongsListView.ItemsSource);


                switch (header.Column.Header.ToString())
                {
                    case "Song": property = "TrackTitle";
                        break;
                    case "Artist": property = "Artist";
                        break;
                    case "Album": property = "Album";
                        break;
                    default:
                        if (header.Column.Header.ToString().StartsWith("Estimated"))
                            property = "EstimatedSongLength";
                        break;
                }
            }
            else if (sender == FriendsListView)
            {
                view = CollectionViewSource.GetDefaultView(FriendsListView.ItemsSource);


                switch (header.Column.Header.ToString())
                {
                    case "UserName": property = "UserName";
                        break;
                    case "Status": property = "Status";
                        break;
                }
            }

            if (view == null) return;

            if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == property)
                if (view.SortDescriptions[0].Direction == ListSortDirection.Descending)
                {
                    var item = new SortDescription(property, ListSortDirection.Ascending);
                    view.SortDescriptions.RemoveAt(0);
                    view.SortDescriptions.Add(item);
                }
                else
                {
                    var item = new SortDescription(property, ListSortDirection.Descending);
                    view.SortDescriptions.RemoveAt(0);
                    view.SortDescriptions.Add(item);
                }
            else
            {
                if (property == null) return;

                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(property, ListSortDirection.Ascending));
            }
        }

        private void aboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.Owner = this;
            aw.ShowDialog();
            aw.Close();
        }

        private void LikedSongsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LikedSongsListView.SelectedItem == null) return;

            SongData b = (SongData)LikedSongsListView.SelectedItem;

            Hanasu.Windows.SongInfoWindow siw = new Windows.SongInfoWindow();
            siw.DataContext = b;

            LikedSongsListView.SelectedItem = null;

            siw.Owner = this;

            siw.ShowDialog();
            siw.Close();
        }

        private void DeleteSelectedLikeItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (LikedSongsListView.SelectedItems != null)
            {

                ConfirmDeleteWindow cdw = new ConfirmDeleteWindow("TrackTitle");
                cdw.Owner = this;
                cdw.DataContext = LikedSongsListView.SelectedItems;


                if ((bool)cdw.ShowDialog())
                {
                    //this is to prevent the 'collecton was modified' exception.
                    ArrayList items = new ArrayList();

                    foreach (var i in LikedSongsListView.SelectedItems)
                        items.Add(i);

                    foreach (var song in items)
                        Hanasu.Services.LikedSongs.LikedSongService.Instance.LikedSongs.Remove((SongData)song);
                }
            }
        }

        private void AddLikedSongFromFileBtn_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            OpenFileDialog odf = new OpenFileDialog();
            odf.Filter = "Audio Files (*.mp3;*.wav;*.wma)|*.mp3;*.wma;*.wav";
            odf.FilterIndex = 1;
            if (odf.ShowDialog() == true)
                Hanasu.Data.ID3.ID3Parser.Parse(odf.FileName);
#else
            MessageBox.Show("This was not implemented.");
#endif
        }

        private void LikedSongsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //http://svetoslavsavov.blogspot.com/2009/09/sorting-and-filtering-databound.html

            // Get the default view from the listview
            ICollectionView view = CollectionViewSource.GetDefaultView(LikedSongsListView.ItemsSource);

            if (view == null) return;

            view.Filter = null;
            view.Filter = new Predicate<object>(t =>
                {
                    var item = (SongData)t;
                    if (item.TrackTitle == null) return false;

                    string textFilter = LikedSongsSearchBox.Text;

                    if (textFilter.Trim().Length == 0) return true; // the filter is empty - pass all items

                    // apply the filter
                    if (item.TrackTitle.ToLower().Contains(textFilter.ToLower()) || item.Artist.ToLower().Contains(textFilter.ToLower()) || (item.Album == null ? false : item.Album.ToLower().Contains(textFilter.ToLower()))) return true;
                    return false;

                });

        }

        private void StationsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the default view from the listview
            ICollectionView view = CollectionViewSource.GetDefaultView(StationsListView.ItemsSource);

            if (view == null) return;

            view.Filter = null;
            view.Filter = new Predicate<object>(t =>
            {
                var item = (Station)t;
                if (item.Name == null) return false;

                string textFilter = StationsSearchBox.Text;

                if (textFilter.Trim().Length == 0) return true; // the filter is empty - pass all items

                // apply the filter
                if (item.Name.ToLower().Contains(textFilter.ToLower()) || item.City.ToLower().Contains(textFilter.ToLower())) return true;
                return false;

            });
        }

        private void StationsListViewRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Stations.StationsService.Instance.DownloadStationsToCacheAsync().ContinueWith(
                t =>
                {
                    Hanasu.Services.Stations.StationsService.Instance.LoadStationsFromRepo();
                });

            CoolDown(StationsListViewRefreshBtn, 0.5);
        }

        private void CoolDown(Control c, double mins)
        {
            var initialTooltip = c.ToolTip;

            Timer cooldown = new Timer();
            ElapsedEventHandler hnd = null;
            hnd = new ElapsedEventHandler((t, i) =>
            {
                cooldown.Elapsed -= hnd;
                cooldown.Stop();
                cooldown.Dispose();

                Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                {
                    c.ToolTip = initialTooltip;
                    c.IsEnabled = true;
                }));
            });
            cooldown.Elapsed += hnd;
            cooldown.Interval = (1000 * 60) * mins; // 3 minutes
            cooldown.Start();

            c.IsEnabled = false;
            c.ToolTip = "Cooling down...";
        }

        private bool HasHandledArgs = false;
        private void StationsListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            try
            {
                if (((App)App.Current).Arguments != null && HasHandledArgs == false)
                {
                    var args = ((App)App.Current).Arguments;

                    if (args.Length == 0) return;

                    if (args[0].ToLower() == "/play_station" && StationsListView.ItemsSource != null)
                    {
                        if (StationsListView.Items.Count > 0) //Relying on there always being more than one item after this has updated.
                        {
                            foreach (Station s in StationsListView.ItemsSource)
                            {
                                if (s.Name == args[1])
                                {
                                    StationsListView.SelectedItem = s;
                                    StationsListView_MouseDoubleClick(StationsListView, null);
                                    break;
                                }

                            }
                            HasHandledArgs = true;
                        }

                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void AddRawSongToLikedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Hanasu.Services.LikedSongs.LikedSongService.Instance.IsSongLikedFromString(SongDataLbl.Text) && !SongDataLbl.Text.Contains(currentStation.Name))
            {
                SongData dat = new SongData();
                dat.OriginallyPlayedStation = currentStation;
                dat.OriginallyBroadcastSongData = SongDataLbl.Text;

                AddEditRawSongWindow ars = new AddEditRawSongWindow();
                ars.Owner = this;
                ars.DataContext = dat;

                bool res = (bool)ars.ShowDialog();
                ars.Close(); //free memory;

                if (res)
                {
                    //if okay was pressed.

                    dat = (SongData)ars.DataContext; //Get edited version back.

                    if (SongDataLbl.Text == dat.OriginallyBroadcastSongData)
                    {
                        //make sure the song hasn't change since opening this dialog.

                        SongIsLiked = true;
                        LikeBtnInfo.IsEnabled = false;
                        AddRawSongToLikedBtn.IsEnabled = false;
                    }

                    currentSong = dat;

                    Hanasu.Services.Events.EventService.RaiseEvent(Services.Events.EventType.Song_Liked,
                    new SongLikedEventInfo()
                    {
                        CurrentSong = dat,
                        CurrentStation = currentStation
                    });

                    //StartSongTimeMeasure();
                }
            }
            else
            {
                AddRawSongToLikedBtn.IsEnabled = false;
            }
        }

        private void thisWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var x = thisWindow.ActualWidth / 2 - (MediaControlPanel.ActualWidth / 2);
            NowPlayingGrid.Width = x;
            SongDataLbl.Width = x - 30;
        }

        private void StationsListViewViewScheduleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (StationsListView.SelectedItem != null)
            {
                var station = (Station)StationsListView.SelectedItem;
                ShowStationScheduleIfAny(station);
            }
        }

        private void ShowStationScheduleIfAny(Station station)
        {
            if (Hanasu.Services.Schedule.ScheduleService.Instance.StationHasSchedule(station))
            {
                Window w = new Window();
                w.Content = Hanasu.Services.Schedule.ScheduleService.Instance.GetSuitableViewingControl(station);
                w.Show();
            }
        }

        private void StationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StationsListView.SelectedItem == null)
            {
                StationsListViewViewHomepageMenuItem.IsEnabled = false;
                StationsListViewViewScheduleMenuItem.IsEnabled = false;
            }
            else
            {
                var station = (Station)StationsListView.SelectedItem;

                if (station.Homepage == null)
                    StationsListViewViewHomepageMenuItem.IsEnabled = false;
                else
                    StationsListViewViewHomepageMenuItem.IsEnabled = true;

                if (station.ScheduleUrl == null)
                    StationsListViewViewScheduleMenuItem.IsEnabled = false;
                else
                    StationsListViewViewScheduleMenuItem.IsEnabled = true;
            }
        }

        private void AddFriendToFriendsListBtn_Click(object sender, RoutedEventArgs e)
        {
            var afd = new AddFriendWindow();
            afd.Owner = this;

            if ((bool)afd.ShowDialog())
            {
                Hanasu.Services.Friends.FriendsService.Instance.AddFriend(
                    afd.TextBoxUserName.Text,
                    afd.IPBox.Text,
                    int.Parse(
                        afd.TextBoxKey.Text), (bool)afd.UDPRadioBtn.IsChecked, (bool)afd.TCPHostRadioBtn.IsChecked);
            }

            afd.Close();
        }

        private void SendMessageFriendSelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FriendsListView.SelectedItem != null)
            {
                var friend = (FriendView)FriendsListView.SelectedItem;
                var win = Hanasu.Services.Friends.FriendsService.Instance.GetChatWindow(friend);
                win.Show();
            }
        }

        private void DeleteSelectedFriendItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FriendsListView.SelectedItems != null)
            {
                ConfirmDeleteWindow cdw = new ConfirmDeleteWindow("UserName");
                cdw.Owner = this;
                cdw.DataContext = FriendsListView.SelectedItems;


                if ((bool)cdw.ShowDialog())
                {
                    List<FriendView> x = new List<FriendView>();
                    foreach (FriendView item in FriendsListView.SelectedItems)
                    {
                        x.Add(item);
                    }

                    Hanasu.Services.Friends.FriendsService.Instance.DeleteFriends(x);
                }
            }
        }

        private void SetAvatarUrlBtn_Click(object sender, RoutedEventArgs e)
        {
            SetAvatarUrlWindow win = new SetAvatarUrlWindow();
            win.Owner = this;
            win.ShowDialog();
            win.Close();
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedItem == LikedTab && likedSongsInitialLoad == false)
            {
                //Delayed loading of the Liked Songs tab.

                LikedSongsListView.IsEnabled = false;
                LikedSongsSearchBox.IsEnabled = false;

                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(t =>
                {
                    System.Threading.Thread.Sleep(550);


                    Dispatcher.Invoke(new EmptyDelegate(() =>
                    {
                        //var be = BindingOperations.GetBindingExpression(LikedSongsListView, ListView.ItemsSourceProperty);

                        //be.UpdateTarget();

                        var b = new Binding();
                        b.Source = Hanasu.Services.LikedSongs.LikedSongService.Instance;
                        b.Path = new PropertyPath("LikedSongs");
                        b.IsAsync = true;
                        b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                        BindingOperations.SetBinding(LikedSongsListView, ListView.ItemsSourceProperty, b);

                        LikedSongsListView.IsEnabled = true;
                        LikedSongsSearchBox.IsEnabled = true;

                        likedSongsInitialLoad = true;
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }));
            }
            else if (tabControl1.SelectedItem == FriendsTab && friendsInitialLoad == false)
            {
                //ItemsSource="{Binding Source={x:Static friends:FriendsService.Instance}, Path=Friends, Mode=OneWay, IsAsync=True, UpdateSourceTrigger=PropertyChanged}"

                //Delayed loading of the Friends tab.

                FriendsListView.IsEnabled = false;
                FriendsSearchTextBox.IsEnabled = false;

                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(t =>
                {
                    System.Threading.Thread.Sleep(550);


                    Dispatcher.Invoke(new EmptyDelegate(() =>
                    {
                        //var be = BindingOperations.GetBindingExpression(LikedSongsListView, ListView.ItemsSourceProperty);

                        //be.UpdateTarget();

                        try
                        {
                            var b = new Binding();
                            b.Source = Hanasu.Services.Friends.FriendsService.Instance;
                            b.Path = new PropertyPath("Friends");
                            b.IsAsync = true;
                            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                            BindingOperations.SetBinding(FriendsListView, ListView.ItemsSourceProperty, b);
                        }
                        catch (Exception)
                        {
                        }

                        FriendsListView.IsEnabled = true;
                        FriendsSearchTextBox.IsEnabled = true;

                        friendsInitialLoad = true;
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }));
            }
        }

        private bool friendsInitialLoad = false;
        private bool likedSongsInitialLoad = false;

        private void FriendsSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the default view from the listview
            ICollectionView view = CollectionViewSource.GetDefaultView(FriendsListView.ItemsSource);

            view.Filter = null;
            view.Filter = new Predicate<object>(t =>
            {
                var item = (FriendView)t;
                if (item.UserName == null) return false;

                string textFilter = FriendsSearchTextBox.Text;

                if (textFilter.Trim().Length == 0) return true; // the filter is empty - pass all items

                // apply the filter
                if (item.UserName.ToLower().Contains(textFilter.ToLower()) || item.Status.ToLower().Contains(textFilter.ToLower())) return true;
                return false;

            });
        }

        private void FriendsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SendMessageFriendSelectedMenuItem_Click(sender, null);
        }

        private void CopyExternalIPBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Hanasu.Services.Friends.FriendsService.Instance.ExternalIP.Replace("\n", ""));
            }
            catch (Exception)
            {
            }
        }

        private void EditSongInfoLikedItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (LikedSongsListView.SelectedItem != null)
            {
                SongData dat = (SongData)LikedSongsListView.SelectedItem;

                var dialog = new AddEditRawSongWindow();
                dialog.DataContext = dat;
                dialog.Owner = this;

                if ((bool)dialog.ShowDialog())
                {
                    SongData dat2 = (SongData)dialog.DataContext;
                    Hanasu.Services.LikedSongs.LikedSongService.Instance.LikedSongs[Hanasu.Services.LikedSongs.LikedSongService.Instance.LikedSongs.IndexOf(dat)] = dat2;
                }
            }
        }

        private void CatalogTabBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CatalogTabBtn.IsEnabled) return;

            var binding = new Binding();
            binding.Source = Hanasu.Services.Stations.StationsService.Instance;
            binding.Path = new PropertyPath("Stations");
            binding.Mode = BindingMode.OneWay;
            binding.IsAsync = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.NotifyOnTargetUpdated = true;

            BindingOperations.SetBinding(StationsListView, ListView.ItemsSourceProperty, binding);

            //ItemsSource="{Binding Source={x:Static stations:StationsService.Instance}, Path=Stations, Mode=OneWay, IsAsync=True, UpdateSourceTrigger=PropertyChanged,NotifyOnTargetUpdated=True}"

            StationsListViewDeleteCustomStationsMenuItem.IsEnabled = false;
            StationsListViewEditCustomStationsMenuItem.IsEnabled = StationsListViewDeleteCustomStationsMenuItem.IsEnabled;
            CustomTabBtn.IsEnabled = true;
            CatalogTabBtn.IsEnabled = false;
        }

        private void CustomTabBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CustomTabBtn.IsEnabled) return;


            var binding = new Binding();
            binding.Source = Hanasu.Services.Stations.StationsService.Instance;
            binding.Path = new PropertyPath("CustomStations");
            binding.Mode = BindingMode.OneWay;
            binding.IsAsync = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.NotifyOnTargetUpdated = true;

            BindingOperations.SetBinding(StationsListView, ListView.ItemsSourceProperty, binding);


            StationsListViewDeleteCustomStationsMenuItem.IsEnabled = true;
            StationsListViewEditCustomStationsMenuItem.IsEnabled = StationsListViewDeleteCustomStationsMenuItem.IsEnabled;
            CatalogTabBtn.IsEnabled = true;
            CustomTabBtn.IsEnabled = false;
        }

        private void StationsListViewCustomStationsAddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddEditStationWindow aesw = new AddEditStationWindow();

            var station = new Station();

            aesw.DataContext = station;

            aesw.Owner = this;

            if ((bool)aesw.ShowDialog())
            {
                station = (Station)aesw.DataContext;

                Hanasu.Services.Stations.StationsService.Instance.CustomStations.Add(station);
            }
        }

        private void StationsListViewDeletecustomStationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CustomTabBtn.IsEnabled) return;

            if (StationsListView.SelectedItems.Count > 0)
            {

                ConfirmDeleteWindow cdw = new ConfirmDeleteWindow("Name");
                cdw.DataContext = StationsListView.SelectedItems;

                if ((bool)cdw.ShowDialog())
                {
                    object[] items = new object[StationsListView.SelectedItems.Count];
                    StationsListView.SelectedItems.CopyTo(items, 0);

                    foreach (Station item in items)
                        Hanasu.Services.Stations.StationsService.Instance.CustomStations.Remove(item);
                }
            }
        }

        private void StationsListViewEditCustomStationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CustomTabBtn.IsEnabled) return;

            if (StationsListView.SelectedItem != null)
            {
                var station = (Station)StationsListView.SelectedItem;
                AddEditStationWindow aesw = new AddEditStationWindow();
                aesw.DataContext = station;
                aesw.Owner = this;

                if ((bool)aesw.ShowDialog())
                {
                    var newData = (Station)aesw.DataContext;
                    Hanasu.Services.Stations.StationsService.Instance.CustomStations[Hanasu.Services.Stations.StationsService.Instance.CustomStations.IndexOf(station)] = newData;
                }
            }
        }

        private void StationHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            StationSongHistoryWindow sss = new StationSongHistoryWindow();
            sss.Owner = this;
            sss.DataContext = Hanasu.Services.Stations.StationsService.GetShoutcastStationSongHistory(currentStation, currentStationAttributes);
            sss.ShowDialog();
            sss.Close();
        }
    }
}
