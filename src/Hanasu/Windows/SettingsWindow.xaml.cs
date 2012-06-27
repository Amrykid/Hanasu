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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Hanasu.Services.Settings;
using System.Diagnostics;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.Unloaded += SettingsWindow_Unloaded;
        }

        void SettingsWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.fbpostSwitch.IsEnabledChanged -= new System.Windows.DependencyPropertyChangedEventHandler(this.fbpostSwitch_IsEnabledChanged);

            this.ReauthFBBtn.Click -= new System.Windows.RoutedEventHandler(this.ReauthFBBtn_Click);

            this.ViewCacheBtn.Click -= new System.Windows.RoutedEventHandler(this.ViewCacheBtn_Click);
            this.UpdateCatalogBtn.Click -= new System.Windows.RoutedEventHandler(this.UpdateCatalogBtn_Click);

            this.OkBtn.Click -= new System.Windows.RoutedEventHandler(this.OkBtn_Click);

            this.CancelBtn.Click -= new System.Windows.RoutedEventHandler(this.CancelBtn_Click);
            this.Unloaded -= SettingsWindow_Unloaded;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.Instance.AutomaticallyFetchSongData = (bool)fetchSongDataSwitch.IsChecked;
            SettingsService.Instance.UpdateStationsLive = (bool)LiveStationUpdSwitch.IsChecked;

            Hanasu.Services.Facebook.FacebookService.FacebookEnabled = ((bool)fetchSongDataSwitch.IsChecked == true ? (bool)fbpostSwitch.IsChecked : false);

            //checks if auth is needed.

            if (Hanasu.Services.Facebook.FacebookService.FacebookEnabled && Hanasu.Services.Facebook.FacebookService.Instance.NeedsToAuth)
            {
                this.Hide();
                Hanasu.Services.Facebook.FacebookService.DoFirstTimeAuth();
                this.Show();
            }

            try
            {
                this.DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                this.Close();
            }
        }

        private void fbpostSwitch_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //didn't want to do it this way but XAML was acting stupid today.

            if (fetchSongDataSwitch.IsChecked == false && fbpostSwitch.IsEnabled == false)
                fbpostSwitch.IsChecked = false;
        }

        private void ReauthFBBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reauthenicate Hanasu with your Facebook profile? You should only do this if you are receiving errors when you are trying to post songs to your profile!", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Press okay (the check button) on the settings window and the authenication window will show.");
                Hanasu.Services.Facebook.FacebookService.FBAccessToken = "";
            }
        }

        private void ViewCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Hanasu.Services.Stations.StationsService.Instance.StationsCacheDir);
        }

        private void UpdateCatalogBtn_Click(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Stations.StationsService.Instance.DownloadStationsToCache();

            MessageBox.Show("The updated catalog will be downloaded in the background. Your stations listing will refresh on restart.");

            UpdateCatalogBtn.IsEnabled = false;
        }

    }
}
