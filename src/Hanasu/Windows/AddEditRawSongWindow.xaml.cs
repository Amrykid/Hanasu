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
using System.Threading;
using Hanasu.Services.Friends;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AddEditRawSongWindow.xaml
    /// </summary>
    public partial class AddEditRawSongWindow : MetroWindow
    {
        public AddEditRawSongWindow()
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
                var a = ((SongData)this.DataContext);

                if (!string.IsNullOrEmpty(RawSongBox.Text))
                {

                    var str = RawSongBox.Text;

                    var clean = Hanasu.Services.Song.SongService.CleanSongDataStr(str);

                    var bits = clean.Split(new string[] { " - " }, StringSplitOptions.None);

                    a.Artist = bits[0];
                    a.TrackTitle = bits[1];

                    this.DataContext = a;
                }

                var dialog = AsyncTaskDialog.GetNewTaskDialog(this, "Fetching additional information",
                    "Please wait. Attempting to grab album information.");

                ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                    {

                        if (a.TrackTitle != null && a.Artist != null)
                            Hanasu.Services.Song.SongService.DataSource.GetAlbumInfo(ref a);

                        Dispatcher.Invoke(new EmptyDelegate(() =>
                            {
                                this.DataContext = a;
                                dialog.Close();
                            }));
                    }));
                dialog.ShowDialog();
            }
            catch (Exception)
            {
            }
        }
    }
}
