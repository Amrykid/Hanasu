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
using System.Reflection;
using MahApps.Metro.Controls;
using System.Diagnostics;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : MetroWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            label2.Content = "Build Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Unloaded += new RoutedEventHandler(AboutWindow_Unloaded);
        }

        void AboutWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.XAMPPLink.RequestNavigate -= new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            this.PhalanxiaLink.RequestNavigate -= new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            this.dallbeeLink.RequestNavigate -= new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            this.TobiLink.RequestNavigate -= new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            this.Unloaded -= new RoutedEventHandler(AboutWindow_Unloaded);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
