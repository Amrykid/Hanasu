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
using System.Text.RegularExpressions;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AddRawSongToLikedWindow.xaml
    /// </summary>
    public partial class AddFriendWindow : MetroWindow
    {
        public AddFriendWindow()
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
            int x = 0;
            if (String.IsNullOrEmpty(TextBoxUserName.Text) || String.IsNullOrEmpty(TextBoxKey.Text) || String.IsNullOrEmpty(IPBox.Text) || String.IsNullOrWhiteSpace(TextBoxUserName.Text) || String.IsNullOrWhiteSpace(TextBoxKey.Text) || String.IsNullOrWhiteSpace(IPBox.Text))
            {
                MessageBox.Show("UserName/Key/IP cannot be empty!");
                return;
            }
            else if (int.TryParse(TextBoxKey.Text, out x) == false)
            {
                MessageBox.Show("A key must be a number!");

                TextBoxKey.Focus();
                TextBoxKey.SelectAll();

                return;
            }
            else if (!Regex.IsMatch(IPBox.Text, ValidIpAddressRegex ,RegexOptions.Compiled | RegexOptions.Singleline))
            {
                MessageBox.Show("Invalid ip address!");

                IPBox.Focus();
                IPBox.SelectAll();

                return;
            }
            else if (Hanasu.Services.Friends.FriendsService.Instance.ContainsFriendByName(TextBoxUserName.Text))
            {
                MessageBox.Show("A friend by this name already exist!");

                TextBoxUserName.Focus();
                TextBoxUserName.SelectAll();

                return;
            }

            TextBoxUserName.Text = TextBoxUserName.Text.Trim();
            IPBox.Text = IPBox.Text.Trim();

            DialogResult = true;
        }

        private const string ValidIpAddressRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"; //http://stackoverflow.com/questions/106179/regular-expression-to-match-hostname-or-ip-address
    }
}
