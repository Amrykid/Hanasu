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
    public partial class ConfirmDeleteWindow : MetroWindow
    {
        public ConfirmDeleteWindow(string DisplayMember = "")
        {
            DisplayMemberPath = DisplayMember;

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

            DialogResult = true;
        }

        public string DisplayMemberPath { get; set; }
    }
}
