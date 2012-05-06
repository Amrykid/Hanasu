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

            this.DialogResult = true;
        }
    }
}
