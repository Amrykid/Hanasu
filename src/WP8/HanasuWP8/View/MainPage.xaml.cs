using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HanasuWP8.Resources;
using Microsoft.Phone.BackgroundAudio;
using System.Threading.Tasks;
using Hanasu.Model;

namespace HanasuWP8.View
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.IsIndeterminate = true;

            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            if (playBtn == null)
                playBtn = ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().First(x => x.Text == "Play" || x.Text == "Pause");

            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.BufferingStarted:
                    {
                        //SystemTray.ProgressIndicator.IsIndeterminate = true;
                        //SystemTray.ProgressIndicator.IsVisible = true;
                        //SystemTray.ProgressIndicator.Text = "Buffering...";
                        break;
                    }
                default:
                    {
                        //SystemTray.ProgressIndicator.IsIndeterminate = true;
                        //SystemTray.ProgressIndicator.IsVisible = false;


                        playBtn.IconUri = new Uri("Images/transport.play.png", UriKind.Relative);
                        playBtn.Text = "Play";

                        break;
                    }
                case PlayState.Playing:
                    {
                        playBtn.IconUri = new Uri("Images/transport.pause.png", UriKind.Relative);
                        playBtn.Text = "Pause";

                        break;
                    }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Instance.Track = new AudioTrack(null, "Moo", "", "", null, "http://173.192.205.178:80", EnabledPlayerControls.Pause);

            //HanasuWP8.AudioPlaybackAgent.AudioPlayer.CurrentStream = "http://173.244.196.108:80/;stream.mp3";

            //Microsoft.Phone.BackgroundAudio.BackgroundAudioPlayer.Instance.Play();
            //HanasuWP8.AudioStreamAgent.AudioTrackStreamer asa = new HanasuWP8.AudioStreamAgent.AudioTrackStreamer();
            //HanasuWP8.AudioPlaybackAgent.AudioPlayer ap = new AudioPlaybackAgent.AudioPlayer();
        }

        private void stationsListBox_DoubleTap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {

            ((HanasuWP8.ViewModel.MainPageStationsViewModel)stationsListBox.DataContext).PlayStationCommand.Execute((Station)stationsListBox.SelectedItem);

        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            //not MVVM-ish. will fix later.

            playBtn = sender as ApplicationBarIconButton;

            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
            {
                if (BackgroundAudioPlayer.Instance.CanPause)
                {
                    //playBtn.IconUri = new Uri("Images/transport.play.png", UriKind.Relative);
                    //playBtn.Text = "Play";

                    BackgroundAudioPlayer.Instance.Pause();
                }
            }
            else
            {
                if (BackgroundAudioPlayer.Instance.Track == null) return;

                //playBtn.IconUri = new Uri("Images/transport.pause.png", UriKind.Relative);
                //playBtn.Text = "Pause";

                BackgroundAudioPlayer.Instance.Play();
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}