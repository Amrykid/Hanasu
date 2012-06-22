using System;
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
                Hanasu.Services.Events.EventService.Initialize();
                Hanasu.Services.Facebook.FacebookService.Initialize();

                Hanasu.Services.Settings.SettingsService.Initialize();

                Hanasu.Services.Logging.LogService.Initialize();


                Hanasu.Services.Stations.StationsService.Initialize();
                Hanasu.Services.Stations.StationsService.Instance.StationFetchStarted += Instance_StationFetchStarted;
                Hanasu.Services.Stations.StationsService.Instance.StationFetchCompleted += Instance_StationFetchCompleted;

                HandleMediaKeyHooks();

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
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            ErrorWindow ew = new ErrorWindow();
            ew.DataContext = e.Exception;

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

                                    break;
                                }
                            case System.Windows.Forms.Keys.MediaNextTrack:
                                {
                                    NextStation();
                                    break;
                                }
                            case System.Windows.Forms.Keys.MediaPreviousTrack:
                                {
                                    PreviousStation();
                                    break;
                                }
                            case System.Windows.Forms.Keys.MediaStop: player.Ctlcontrols.stop();
                                break;

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
            //See HandleMediaKeyHooks

            /*
            switch (e.Key)
            {
                case Key.MediaPlayPause:
                    {
                        if (NowPlayingGrid.Visibility == System.Windows.Visibility.Visible)
                            player.Ctlcontrols.pause();
                        else
                            player.Ctlcontrols.play();

                        break;
                    }
                case Key.MediaStop: player.Ctlcontrols.stop();
                    break;

            }
             * */
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {

            Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue();

            this.KeyUp -= MainWindow_KeyUp;

            player.PlayStateChange -= player_PlayStateChange;
            player.MediaChange -= player_MediaChange;
            player.MediaError -= player_MediaError;
            player.ScriptCommand -= player_ScriptCommand;
            player.MarkerHit -= player_MarkerHit;
            player.EndOfStream -= player_EndOfStream;
            player.CurrentMediaItemAvailable -= player_CurrentMediaItemAvailable;

            bufferTimer.Elapsed -= bufferTimer_Elapsed;
            bufferTimer.Dispose();

            player.close();
            player.Dispose();

            this.Loaded -= MainWindow_Loaded;
            this.Unloaded -= MainWindow_Unloaded;
        }

        private AxWMPLib.AxWindowsMediaPlayer player = null;
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //configure the wmp control

            windowsFormsHost1.Child = new Hanasu.Core.AxWMP();
            player = ((Hanasu.Core.AxWMP)windowsFormsHost1.Child).axWindowsMediaPlayer1;

            player.Dock = System.Windows.Forms.DockStyle.Fill;

            player.uiMode = "none";
            player.enableContextMenu = false;
            player.settings.autoStart = false;

            player.stretchToFit = true;

            player.MediaError += player_MediaError;

            player.PlayStateChange += player_PlayStateChange;

            player.MediaChange += player_MediaChange;

            player.ScriptCommand += player_ScriptCommand;

            player.CurrentMediaItemAvailable += player_CurrentMediaItemAvailable;

            player.MarkerHit += player_MarkerHit;

            player.EndOfStream += player_EndOfStream;

            VolumeSlider.Value = player.settings.volume;

            HandleWindowsTaskbarstuff();

            bufferTimer = new Timer();
            bufferTimer.Elapsed += bufferTimer_Elapsed;

            bufferTimer.Interval = 1000;

            currentStationAttributes = new Hashtable();

            this.tabControl1.SelectedIndex = 1;
        }

        void player_CurrentMediaItemAvailable(object sender, AxWMPLib._WMPOCXEvents_CurrentMediaItemAvailableEvent e)
        {

        }

        void player_EndOfStream(object sender, AxWMPLib._WMPOCXEvents_EndOfStreamEvent e)
        {

        }

        void player_MarkerHit(object sender, AxWMPLib._WMPOCXEvents_MarkerHitEvent e)
        {
            throw new NotImplementedException();
        }

        void player_ScriptCommand(object sender, AxWMPLib._WMPOCXEvents_ScriptCommandEvent e)
        {
            throw new NotImplementedException();
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
        private void HandleWindowsTaskbarstuff()
        {
            //http://joshsmithonwpf.wordpress.com/2007/03/09/how-to-programmatically-click-a-button/

            //check if W7 or higher
            if (
                (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1) /* if windows 7 */
                || Environment.OSVersion.Version.Major > 6 /* if windows 8, etc */ )
            {
                this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                this.TaskbarItemInfo.ThumbButtonInfos = new System.Windows.Shell.ThumbButtonInfoCollection();

                /*
                CommandBinding commandBinding = new CommandBinding(ButtonClickCommand, (object sender2, ExecutedRoutedEventArgs e2) =>
                {
                    playBtn_Click(null, null);
                });
                this.CommandBindings.Add(commandBinding);

                this.TaskbarItemInfo.ThumbButtonInfos.Add(new System.Windows.Shell.ThumbButtonInfo()
                {
                    Description = "play",
                    CommandTarget = playBtn,
                    Command = ButtonClickCommand,
                    ImageSource = (ImageSource)((Image)playBtn.Content).Source,
                }); */
            }
        }

        public static RoutedCommand ButtonClickCommand = new RoutedCommand();
        #endregion


        private Timer bufferTimer = null;

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
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
        private SongData currentSong = null;
        internal Hashtable currentStationAttributes { get; set; }
        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            try
            {
                if (lastMediaTxt != player.currentMedia.name)
                {
                    var name = player.currentMedia.name;

                    if (name.Contains(" - ") && name.Contains(currentStation.Name) == false && name.Split(' ').Length > 1 && !System.Text.RegularExpressions.Regex.IsMatch(name, @"^(http\://)?[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?$")) //cheap way to check if its a song title. not perfect and doesn't work 100% of the time.
                    {
                        MoreInfoBtn.Visibility = System.Windows.Visibility.Hidden;

                        lastMediaTxt = name;

                        SongDataLbl.Text = name;

                        SongIsLiked = false;

                        LikeBtnInfo.IsEnabled = true;

                        //song changed. maybe a couple of seconds late.

                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                            name, 4000, false, Services.Notifications.NotificationType.Now_Playing);

                        if (Hanasu.Services.Settings.SettingsService.Instance.AutomaticallyFetchSongData)
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    var n = name;
                                    var stat = currentStation;

                                    System.Threading.Thread.Sleep(1000 * 20); //wait 20 seconds. if the user is still listening to the station, pull the lyrics. this creates less stress/http request to the lyrics site.

                                    if (stat != currentStation)
                                        return;

                                    if (stat.StationType == StationType.Radio)
                                    {

                                        Uri lyricsUrl = null;
                                        if (Hanasu.Services.Song.SongService.IsSongAvailable(name, out lyricsUrl))
                                        {
                                            if ((bool)Dispatcher.Invoke(
                                                   new Hanasu.Services.Notifications.NotificationsService.EmptyReturnDelegate(() =>
                                                   {
                                                       return (n != SongDataLbl.Text);

                                                   })))
                                                return;

                                            Hanasu.Services.Notifications.NotificationsService.AddNotification(name.Substring(0, name.Length / 2) + "..." + " - Song info found",
                                            "Lyrics and other data found for this song.", 4000, false, Services.Notifications.NotificationType.Music_Data);

                                            Dispatcher.Invoke(
                                                new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                                    {
                                                        MoreInfoBtn.Visibility = System.Windows.Visibility.Visible;

                                                        currentSong = Hanasu.Services.Song.SongService.GetSongData(name);

                                                        MoreInfoBtn.DataContext = currentSong;
                                                    }));
                                        }
                                    }
                                }).ContinueWith((tk) => tk.Dispose());
                    }
                    else
                    {
                        Dispatcher.Invoke(
                                            new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                            {
                                                MoreInfoBtn.Visibility = System.Windows.Visibility.Hidden;
                                            }));

                        //since its not a song, might as well display it as a radio message instead of 'Now Playing'.

                        currentSong = null;

                        SongIsLiked = false;

                        LikeBtnInfo.IsEnabled = false;

                        lastMediaTxt = name;

                        SongDataLbl.Text = "Not Available";

                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - " + (currentStation.StationType == StationType.Radio ? "Radio Message" : "TV Message"),
                            name, 4000, false, Services.Notifications.NotificationType.Information);
                    }
                }
            }
            catch (Exception)
            {
            }

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
                    break;
                case WMPLib.WMPPlayState.wmppsPlaying: NowPlayingGrid.Visibility = System.Windows.Visibility.Visible;
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;
                    bufferTimer.Stop();
                    playBtn.IsEnabled = false;
                    pauseBtn.IsEnabled = true;

                    unableToConnectNotificationShown = false;

                    player.settings.mute = isMuted;

                    if (!player.settings.mute)
                    {
                        VolumeSlider.IsEnabled = true;
                    }
                    VolumeMuteBtn.IsEnabled = true;

                    HideStationsAdorner(); //Playing, hide the adorner and rename the listview.

                    currentStationAttributes.Clear();
                    for (int i = 0; i < player.currentMedia.attributeCount; i++)
                    {
                        var x = player.currentMedia.getAttributeName(i);
                        var y = player.currentMedia.getItemInfo(x);

                        currentStationAttributes.Add(x, y);
                    }

                    OnPropertyChanged("currentStationAttributes");

                    //for (int i = 0; i < player.currentPlaylist.count; i++)
                    //{
                    //    var x = player.currentPlaylist.get_Item(i);
                    //    var y = player.currentPlaylist.getItemInfo(x.name);

                    //    var z = 6;
                    //}
                    //for (int i = 0; i < player.currentPlaylist.attributeCount; i++)
                    //{
                    //    var x = player.currentPlaylist.get_attributeName(i);
                    //    var y = 0;
                    //}

                    break;
                case WMPLib.WMPPlayState.wmppsReady:
                case WMPLib.WMPPlayState.wmppsPaused:
                case WMPLib.WMPPlayState.wmppsStopped: NowPlayingGrid.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = true;
                    pauseBtn.IsEnabled = false;

                    bufferTimer.Stop();
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;

                    unableToConnectNotificationShown = false;

                    VolumeSlider.IsEnabled = false;
                    VolumeMuteBtn.IsEnabled = false;

                    if (Hanasu.Services.Stations.StationsService.Instance.Status != StationsServiceStatus.Polling)
                        HideStationsAdorner();
                    break;
            }
        }

        private Station currentStation = null;

        private void StationsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //get the selected station and play it

            var station = (Station)StationsListView.SelectedItem;

            if (station == null)
                return;

            if (currentStation != station)
                Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue(); //Get rid of messages from the last station, if any.

            if (Hanasu.Services.Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(station.DataSource, station.ExplicitExtension))
            {
                var d = station.Cacheable && station.StationType != StationType.TV && station.LocalStationFile != null ? station.LocalStationFile : station.DataSource;
                //Hanasu.Services.Preprocessor.PreprocessorService.Process(ref d);

                var pro = Hanasu.Services.Preprocessor.PreprocessorService.GetProcessor(d, station.ExplicitExtension);

                //Check if its a multi-stream station.
                if (pro.GetType().BaseType == typeof(Hanasu.Services.Preprocessor.MultiStreamPreprocessor))
                {
                    var p = (Hanasu.Services.Preprocessor.MultiStreamPreprocessor)pro;

                    var entries = p.Parse(d);

                    if (entries.Length == 0)
                    {
                        throw new Exception("No stations found!");
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
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                Hanasu.Services.Preprocessor.PreprocessorService.Process(ref d);

                player.URL = d.ToString();
            }
            else
            {
                if (station.Cacheable && station.StationType != StationType.TV && station.LocalStationFile != null)
                    player.URL = station.LocalStationFile.ToString();
                else
                    player.URL = station.DataSource.ToString();
            }

            LogService.Instance.WriteLog(typeof(MainWindow),
    "Playing station: " + station.Name);

            if (station.StationType == StationType.TV)
                player.network.bufferingTime = 10000; // 10 seconds ahead
            else
                player.network.bufferingTime = 2000; // 2 seconds

            currentStationAttributes.Clear();

            player.Ctlcontrols.play();

            currentStation = station;

            Hanasu.Services.Events.EventService.RaiseEventAsync(Services.Events.EventType.Station_Changed
                , new StationEventInfo()
                {
                    CurrentStation = currentStation
                });

            if (station.StationType == StationType.TV)
                tabControl1.SelectedIndex = 0;
        }

        public class StationEventInfo : Hanasu.Services.Events.EventInfo
        {
            public Station CurrentStation { get; internal set; }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.settings.volume = (int)VolumeSlider.Value;
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            player.Ctlcontrols.play();
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentStation != null)
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
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Owner = this;
            sw.ShowDialog();
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

            LikeBtnInfo.IsEnabled = false;

        }
        public class SongLikedEventInfo : StationEventInfo
        {
            public SongData CurrentSong { get; set; }
        }

        private Lazy<object> StationListViewGridButtonImageValueMusic = new Lazy<object>(new Func<object>(() => (Visual)Application.Current.MainWindow.Resources["appbar_music"]));
        private Lazy<object> StationListViewGridButtonImageValueTV = new Lazy<object>(new Func<object>(() => (Visual)Application.Current.MainWindow.Resources["appbar_tv"]));
        private void StationsListViewGridItemButton_Loaded(object sender, RoutedEventArgs e)
        {
            switch ((StationType)((Button)sender).DataContext)
            {
                case StationType.Radio: ((Button)sender).Content = (Visual)StationListViewGridButtonImageValueMusic.Value;
                    break;
                case StationType.TV: ((Button)sender).Content = (Visual)StationListViewGridButtonImageValueTV.Value;
                    break;
            }
        }

        private void StationAttributesBtn_Click(object sender, RoutedEventArgs e)
        {
            StationAttributesWindow saw = new StationAttributesWindow();
            saw.DataContext = this;
            saw.Owner = this;
            saw.ShowDialog();
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
    }
}
