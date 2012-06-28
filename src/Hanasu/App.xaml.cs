using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using Hanasu.Windows;
using Hanasu.Core;

namespace Hanasu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Hanasu.Windows.SplashScreen SplashScreen { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (NetworkUtils.IsConnectedToInternet())
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\";
                if (!Directory.Exists(dir))
                {
                    //SplashScreen.Close();

                    FirstTimeRunWizard ftrw = new FirstTimeRunWizard();
                    ftrw.ShowDialog();

                    //Restart
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                else
                {
                    //Start the splash screen.
                    SplashScreen = new Windows.SplashScreen();
                    SplashScreen.Show();
                    SplashScreen.Focus();
                }
            }
            else
            {
                MessageBox.Show("Hanasu requires an active internet connection! Will now terminate.");
                Application.Current.Shutdown();
            }
        }
    }
}
