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

            Hanasu.Services.Stations.StationsService.Initialize();

            this.KeyUp += MainWindow_KeyUp;

            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
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
            this.KeyUp -= MainWindow_KeyUp;

            player.PlayStateChange -= player_PlayStateChange;
            player.MediaChange -= player_MediaChange;

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

            player.uiMode = "none";
            player.enableContextMenu = false;
            player.settings.autoStart = false;

            player.PlayStateChange += player_PlayStateChange;

            player.MediaChange += player_MediaChange;

            VolumeSlider.Value = player.settings.volume;
            
        }

        private string lastMediaTxt = null; //prevents the below event from constantly queueing the same song title.
        void player_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            try
            {
                if (lastMediaTxt != player.currentMedia.name)
                {
                    lastMediaTxt = player.currentMedia.name;

                    SongDataLbl.Text = player.currentMedia.name;

                    //song changed. maybe a couple of seconds late.

                    Hanasu.Services.Notifications.NotificationsService.AddNotification(currentStation.Name + " - Now Playing",
                        player.currentMedia.name, 4000);
                }
            }
            catch (Exception)
            {
            }

        }

        void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //handle the wmp's status

            switch (e.newState)
            {
                case (int)WMPLib.WMPPlayState.wmppsBuffering: BufferingSP.Visibility = System.Windows.Visibility.Visible;
                    break;
                case (int)WMPLib.WMPPlayState.wmppsPlaying: NowPlayingGrid.Visibility = System.Windows.Visibility.Visible;
                    BufferingSP.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = false;
                    pauseBtn.IsEnabled = true;
                    break;
                case (int)WMPLib.WMPPlayState.wmppsPaused:
                case (int)WMPLib.WMPPlayState.wmppsStopped: NowPlayingGrid.Visibility = System.Windows.Visibility.Hidden;
                    playBtn.IsEnabled = true;
                    pauseBtn.IsEnabled = false;
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
    }
}
