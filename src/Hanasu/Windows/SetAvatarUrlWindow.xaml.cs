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
    public partial class SetAvatarUrlWindow : MetroWindow
    {
        public SetAvatarUrlWindow()
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
            if (String.IsNullOrEmpty(AvatarUrlBox.Text))
            {
                MessageBox.Show("Url cannot be cannot be empty!");
                return;
            }
            else if (!AvatarUrlBox.Text.StartsWith("http://") && !AvatarUrlBox.Text.StartsWith("https://"))
            {
                MessageBox.Show("Avatar must be available from the internet!");
                return;
            }
            else if (AvatarImage.Source == null)
            {
                return;
            }

            Hanasu.Services.Friends.FriendsService.AvatarUrl = AvatarUrlBox.Text;

            DialogResult = true;
        }
    }
}
