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
using Hanasu.Services.Song;
using System.Diagnostics;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for SongInfoWindow.xaml
    /// </summary>
    public partial class SongInfoWindow : MetroWindow
    {
        public SongInfoWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buyAlbumbtn_Click(object sender, RoutedEventArgs e)
        {
            if (((SongData)this.DataContext).BuyUri != null)
                Process.Start(((SongData)this.DataContext).BuyUri.ToString());
        }

        private void thisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (((SongData)this.DataContext).BuyUri == null)
                buyAlbumbtn.IsEnabled = false;
        }
    }
}
