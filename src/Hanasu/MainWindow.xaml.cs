using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro;
using Hanasu.Services.Stations;
using Hanasu.Services.Logging;
using Hanasu.Core;
using Hanasu.Windows;
using System.ComponentModel;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Hanasu.Services.Settings.SettingsService.Initialize();

                Hanasu.Services.Logging.LogService.Initialize();


                Hanasu.Services.Stations.StationsService.Initialize();
                Hanasu.Services.Stations.StationsService.Instance.StationFetchStarted += Instance_StationFetchStarted;
                Hanasu.Services.Stations.StationsService.Instance.StationFetchCompleted += Instance_StationFetchCompleted;


                //Hanasu.Services.Friends.FriendsService.Initialize();

                this.KeyUp += MainWindow_KeyUp;

                this.Loaded += MainWindow_Loaded;
                this.Unloaded += MainWindow_Unloaded;
            }
            else
            {
                //Is in the designer. Do nothing.
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
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {

            Hanasu.Services.Notifications.NotificationsService.ClearNotificationQueue();

            this.KeyUp -= MainWindow_KeyUp;

            player.PlayStateChange -= player_PlayStateChange;
            player.MediaChange -= player_MediaChange;
            player.MediaError -= player_MediaError;

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

            player.MediaError += player_MediaError;

            player.PlayStateChange += player_PlayStateChange;

            player.MediaChange += player_MediaChange;

            VolumeSlider.Value = player.settings.volume;

        }

        void player_MediaError(object sender, AxWMPLib._WMPOCXEvents_MediaErrorEvent e)
        {
            Hanasu.Services.Notifications.NotificationsService.AddNotification(
                "Unable to connect to station.",
                "Hanasu was unable to connect to " + currentStation.Name + ".", 4000);

            HideStationsAdorner(); //On error rename the stations listview.
        }

        private string lastMediaTxt = null; //prevents the below event from constantly queueing the same song title.
        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            try
            {
                if (lastMediaTxt != player.currentMedia.name)
                {
                    var name = player.currentMedia.name;

                    if (name.Contains(" - ") && name.Contains(currentStation.Name) == false && name.Split(' ').Length > 1) //cheap way to check if its a song title. not perfect and doesn't work 100% of the time.
                    {
                        MoreInfoBtn.Visibility = System.Windows.Visibility.Hidden;

                        lastMediaTxt = name;

                        SongDataLbl.Text = name;

                        //song changed. maybe a couple of seconds late.

                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                            name, 4000);

                        if (Hanasu.Services.Settings.SettingsService.Instance.AutomaticallyFetchSongData)
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    var n = name;
                                    var stat = currentStation;

                                    System.Threading.Thread.Sleep(1000 * 20); //wait 20 seconds. if the user is still listening to the station, pull the lyrics. this creates less stress/http request to the lyrics site.

                                    if (stat != currentStation)
                                        return;

                                    if ((bool)Dispatcher.Invoke(
                                           new Hanasu.Services.Notifications.NotificationsService.EmptyReturnDelegate(() =>
                                               {
                                                   return (n != SongDataLbl.Text);

                                               })))
                                        return;

                                    Uri lyricsUrl = null;
                                    if (Hanasu.Services.Song.SongService.IsSongAvailable(name, out lyricsUrl))
                                    {
                                        Hanasu.Services.Notifications.NotificationsService.AddNotification(name.Substring(0, name.Length / 2) + "..." + " - Song info found",
                                        "Lyrics and other data found for this song.", 4000);

                                        Dispatcher.Invoke(
                                            new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                                {
                                                    MoreInfoBtn.Visibility = System.Windows.Visibility.Visible;
                                                    MoreInfoBtn.DataContext = Hanasu.Services.Song.SongService.GetSongData(name);
                                                }));
                                    }
                                }).ContinueWith((tk) => tk.Dispose());
                    }
                    else
                    {
                        //since its not a song, might as well display it as a radio message instead of 'Now Playing'.

                        lastMediaTxt = name;

                        SongDataLbl.Text = "Not Available";

                        Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Radio Message",
                            name, 4000);
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
                    break;
                case WMPLib.WMPPlayState.wmppsPlaying: NowPlayingGrid.Visibility = System.Windows.Visibility.Visible;
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = false;
                    pauseBtn.IsEnabled = true;

                    HideStationsAdorner(); //Playing, hide the adorner and rename the listview.

                    break;
                case WMPLib.WMPPlayState.wmppsReady:
                case WMPLib.WMPPlayState.wmppsPaused:
                case WMPLib.WMPPlayState.wmppsStopped: NowPlayingGrid.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = true;
                    pauseBtn.IsEnabled = false;

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

            if (Hanasu.Services.Preprocessor.PreprocessorService.CheckIfPreprocessingIsNeeded(station.DataSource))
            {
                var d = station.DataSource;
                Hanasu.Services.Preprocessor.PreprocessorService.Process(ref d);

                player.URL = d.ToString();
            }
            else
            {
                player.URL = station.DataSource.ToString();
            }

            LogService.Instance.WriteLog(typeof(MainWindow),
    "Playing station: " + station.Name);

            player.Ctlcontrols.play();

            currentStation = station;
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
            player.Ctlcontrols.pause();
        }

        private void ShowStationsAdorner()
        {
            LogService.Instance.WriteLog(typeof(MainWindow),
    "Station adorner shown.");

            StationsListAdorner.IsAdornerVisible = true;
            StationsListView.IsEnabled = false;
        }
        private void HideStationsAdorner()
        {
            LogService.Instance.WriteLog(typeof(MainWindow),
    "Station adorner hid.");

            StationsListAdorner.IsAdornerVisible = false;
            StationsListView.IsEnabled = true;
        }

        private void LogListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (LogListView.ItemsSource == null) return;

            //LogListView.ScrollIntoView(LogListView.Items[((ObservableQueue<LogMessage>)LogListView.Items.SourceCollection).Count - 1]);
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
    }
}
