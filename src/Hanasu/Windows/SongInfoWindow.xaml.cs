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
            this.Unloaded += SongInfoWindow_Unloaded;
        }
        void SongInfoWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveBtn.Click -= SaveBtn_Click;
            PrintBtn.Click -= PrintBtn_Click;
            this.thisWindow.Loaded -= new System.Windows.RoutedEventHandler(this.thisWindow_Loaded);
            this.CloseBtn.Click -= new System.Windows.RoutedEventHandler(this.Close_Click);
            this.buyAlbumbtn.Click -= new System.Windows.RoutedEventHandler(this.buyAlbumbtn_Click);
            this.Unloaded -= SongInfoWindow_Unloaded;
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
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);

            var songData = (SongData)this.DataContext;
            if (songData.BuyUri == null)
                buyAlbumbtn.IsEnabled = false;

            if (songData.Lyrics == null)
            {
                SaveBtn.IsEnabled = false;
                PrintBtn.IsEnabled = false;
            }
            else
            {
                SaveBtn.Click += SaveBtn_Click;
                PrintBtn.Click += PrintBtn_Click;
            }
        }

        void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            var songData = (SongData)this.DataContext;

            var diag = new PrintDialog();
            diag.UserPageRangeEnabled = false;

            if (diag.ShowDialog() == true)
            {
                var header = songData.Artist + " - " + songData.TrackTitle + " | Lyrics - Hanasu";

                var doc = new FlowDocument();
                doc.PagePadding = new Thickness(100);

                doc.Blocks.Add(new Paragraph(new Run(header))
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 23
                });

                doc.Blocks.Add(new Paragraph(new Run(songData.Lyrics))
                {
                    TextAlignment = TextAlignment.Center
                });

                diag.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, header);
            }
        }

        void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //Since Forms is already imported and the forms file dialog uses xp visuals, use it.
            using (var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                var songData = (SongData)this.DataContext;

                sfd.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.AutoUpgradeEnabled = true;
                sfd.FileName = String.Format("{0}_{1}_lyrics.txt", songData.Artist, songData.TrackTitle);

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                {
                    System.IO.File.WriteAllText(sfd.FileName, songData.Lyrics);
                }
            }
        }
    }
}
