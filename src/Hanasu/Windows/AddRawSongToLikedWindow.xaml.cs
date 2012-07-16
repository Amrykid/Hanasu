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

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AddRawSongToLikedWindow.xaml
    /// </summary>
    public partial class AddRawSongToLikedWindow : MetroWindow
    {
        public AddRawSongToLikedWindow()
        {
            InitializeComponent();
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
            this.Unloaded += AddRawSongToLikedWindow_Unloaded;
        }

        void AddRawSongToLikedWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            AutoFillBtn.Click -= AutoFillBtn_Click;
            this.Unloaded -= AddRawSongToLikedWindow_Unloaded;
            OkayBtn.Click -= OkayBtn_Click;

            BindingOperations.ClearAllBindings(this);
        }

        private void OkayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxArtist.Text) || String.IsNullOrEmpty(TextBoxTrack.Text))
            {
                MessageBox.Show("Artist/Song Name cannot be empty!");
                return;
            }

            DialogResult = true;
        }

        private void AutoFillBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var str = RawSongBox.Text;

                var clean = Hanasu.Services.Song.SongService.CleanSongDataStr(str);

                var bits = clean.Split(new string[] { " - " }, StringSplitOptions.None);

                TextBoxArtist.Text = bits[0];
                TextBoxTrack.Text = bits[1];
            }
            catch (Exception)
            {
            }
        }
    }
}
