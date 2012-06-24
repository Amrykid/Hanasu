using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

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
            //Start the splash screen.
            SplashScreen = new Windows.SplashScreen();
            SplashScreen.Show();
            SplashScreen.Focus();
        }
    }
}
