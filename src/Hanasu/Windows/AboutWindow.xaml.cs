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
using System.Threading;

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
            this.Focus();
        }

        void AboutWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp -= new KeyEventHandler(this.MetroWindow_KeyUp);
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
        List<Key> konamiCode = new List<Key>();
        private void MetroWindow_KeyUp(object sender, KeyEventArgs e)
        {
            konamiCode.Add(e.Key);

            if (konamiCode.Count == 8)
            {
                var ifCorrect = konamiCode[0] == Key.Up && konamiCode[1] == Key.Up && konamiCode[2] == Key.Down && konamiCode[3] == Key.Down && konamiCode[4] == Key.Left && konamiCode[5] == Key.Right && konamiCode[6] == Key.Left && konamiCode[7] == Key.Right;
                if (ifCorrect)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                        {
                            Window w = null;
                            WebBrowser wb = null;
                            Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                {
                                    w = new Window();
                                    wb = new WebBrowser();
                                    wb.NavigateToString("<object width=\"420\" height=\"315\"><param name=\"movie\" value=\"http://www.youtube.com/v/Jm8Vus-VQwE?version=3&amp;hl=en_US&amp;rel=0&autoplay=1\"></param><param name=\"allowFullScreen\" value=\"true\"></param><param name=\"allowscriptaccess\" value=\"always\"></param><embed src=\"http://www.youtube.com/v/Jm8Vus-VQwE?version=3&amp;hl=en_US&amp;rel=0&autoplay=1\" type=\"application/x-shockwave-flash\" width=\"420\" height=\"315\" allowscriptaccess=\"always\" allowfullscreen=\"true\"></embed></object>");
                                    w.Content = wb;
                                    w.ShowInTaskbar = false;
                                    w.Topmost = true;
                                    w.Show();
                                    w.Hide();
                                }));

                            System.Threading.Thread.Sleep((1000 * 60) * 4);

                            Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                               {
                                   wb.NavigateToString("");
                                   wb.Dispose();
                                   w.Close();
                               }));
                        }));
                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Konami Code Activated!",
                        "So you want to be the very best?!", 3000);
                    konamiCode.Clear();

                    this.Close();
                }

                konamiCode.Clear();
            }
        }
    }
}
