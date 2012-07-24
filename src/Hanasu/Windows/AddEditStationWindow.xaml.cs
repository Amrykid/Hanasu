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
    public partial class AddEditStationWindow : MetroWindow
    {
        public AddEditStationWindow()
        {
            InitializeComponent();
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
            this.Unloaded += AddRawSongToLikedWindow_Unloaded;
        }

        void AddRawSongToLikedWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= AddRawSongToLikedWindow_Unloaded;
            OkayBtn.Click -= OkayBtn_Click;

            BindingOperations.ClearAllBindings(this);
        }

        private void OkayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxName.Text) || String.IsNullOrEmpty(TextBoxUrl.Text))
            {
                MessageBox.Show("Station Name/Url cannot be empty!");
                return;
            }
            else if (!string.IsNullOrEmpty(TextBoxExtension.Text))
            {
                if (!TextBoxExtension.Text.StartsWith("."))
                {
                    MessageBox.Show("Extension must begin with a period!");
                    TextBoxExtension.Focus();
                    return;
                }
            }

            DialogResult = true;
        }

    }
}
