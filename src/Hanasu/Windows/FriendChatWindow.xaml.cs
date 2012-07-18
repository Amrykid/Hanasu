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
using Hanasu.Services.Friends;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AddRawSongToLikedWindow.xaml
    /// </summary>
    public partial class FriendChatWindow : MetroWindow
    {
        private FriendConnection conn = null;
        public FriendChatWindow()
        {
            InitializeComponent();
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
            this.Unloaded += AddRawSongToLikedWindow_Unloaded;
            this.Loaded += FriendChatWindow_Loaded;
            this.Closing += FriendChatWindow_Closing;

            Hanasu.Services.Events.EventService.AttachHandler(Services.Events.EventType.Theme_Changed,
                    e =>
                    {
                        Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
                    });

        }

        void FriendChatWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current == null)
                e.Cancel = false;
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        public string LastMessageReceived { get; set; }
        void FriendChatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            conn = ((FriendView)this.DataContext).Connection;
        }

        void AddRawSongToLikedWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Closing -= FriendChatWindow_Closing;
            this.Loaded -= FriendChatWindow_Loaded;
            this.Unloaded -= AddRawSongToLikedWindow_Unloaded;
            SendBtn.Click -= SendBtn_Click;

            BindingOperations.ClearAllBindings(this);
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = Hanasu.Services.Friends.FriendsService.Instance.Friends.First(t => t.UserName == ((FriendView)this.DataContext).UserName);
            conn = ((FriendView)this.DataContext).Connection;
            if (!conn.IsConnected)
                MessageBox.Show("Not connected!");
            else
            {

                if (string.IsNullOrEmpty(textBox1.Text))
                    return;

                try
                {
                    conn.SendChatMessage(textBox1.Text);

                    if (textBox1.Text.StartsWith("/me ") && textBox1.Text.Length >= 4)
                        ConversationBox.Text += "* " + "You" + " " + textBox1.Text.Substring(4) + Environment.NewLine;
                    else
                        ConversationBox.Text += "<" + "You" + ">: " + textBox1.Text + Environment.NewLine;


                    ConversationBox.ScrollToEnd();
                    textBox1.Clear();
                }
                catch (Exception)
                {
                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Chat Error", "Unable to send message. Peer must be disconnected.");
                }
            }
        }
        public void HandleMessage(string msg)
        {
            if (msg.StartsWith("/me "))
                ConversationBox.Text += "* " + conn.UserName + " " + msg.Substring(4) + Environment.NewLine;
            else
                ConversationBox.Text += "<" + conn.UserName + ">: " + msg + Environment.NewLine;

            ConversationBox.ScrollToEnd();
            LastMessageReceived = msg;
        }

        private void Label_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var fv = (FriendView)this.DataContext;
            if (fv.Connection == null) return;

            textBox1.IsEnabled = !fv.Status.StartsWith("Offline");
        }
    }
}
