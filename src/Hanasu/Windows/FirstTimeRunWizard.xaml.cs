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
using System.Threading;
using MahApps.Metro.Controls;
using System.IO;
using Hanasu.Services.Stations;
using System.ComponentModel;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for FirstTimeRunWizard.xaml
    /// </summary>
    public partial class FirstTimeRunWizard : MetroWindow
    {
        public FirstTimeRunWizard()
        {
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(FirstTimeRunWizard_Unloaded);
        }

        void FirstTimeRunWizard_Unloaded(object sender, RoutedEventArgs e)
        {
            this.NextBtn.Click -= new System.Windows.RoutedEventHandler(this.NextBtn_Click);
            this.CancelBtn.Click -= new System.Windows.RoutedEventHandler(this.CancelBtn_Click);
            this.WizardTabControl.SelectionChanged -= new System.Windows.Controls.SelectionChangedEventHandler(this.WizardTabControl_SelectionChanged);
            this.Unloaded -= new RoutedEventHandler(FirstTimeRunWizard_Unloaded);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Application.Current.Shutdown(1);
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WizardTabControl.SelectedIndex == 0)
                WizardTabControl.SelectedIndex++;
            else
                this.DialogResult = true;
        }

        private void WizardTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (WizardTabControl.SelectedIndex)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        if (!DesignerProperties.GetIsInDesignMode(this))
                        {
                            CancelBtn.IsEnabled = false;
                            NextBtn.IsEnabled = false;

                            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                                {
                                    Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                        {
                                            StatusLabel.Content = "Configuring.... Step 1 of 2.";
                                            StatusPB.Value = 0;
                                            StatusPB.Maximum = 100;
                                        }));

                                    var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Hanasu\\";

                                    if (!System.IO.Directory.Exists(dir))
                                        System.IO.Directory.CreateDirectory(dir);


                                    Hanasu.Services.Settings.SettingsService.Instance.CreateNewSettingsFile(dir + "settings.xml");

                                    Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                    {
                                        StatusLabel.Content = "Configuring.... Step 1 of 2.";
                                        StatusPB.Value = 100;
                                        StatusPB.Maximum = 100;
                                    }));

                                    Thread.Sleep(2000);

                                    Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                    {
                                        StatusLabel.Content = "Pre-downloading station data.... Step 2 of 2.";
                                        StatusPB.Value = 0;
                                    }));

                                    if (!Directory.Exists(Hanasu.Services.Stations.StationsService.StationsCacheDir))
                                        Directory.CreateDirectory(Hanasu.Services.Stations.StationsService.StationsCacheDir);

                                    RadioFormat dummie = 0;


                                    var stats = from x in Hanasu.Services.Stations.StationsService.StreamStationsXml()
                                                select Hanasu.Services.Stations.StationsService.ParseStation(ref dummie, x);

                                    var cacheAble = stats.Where(i => i.Cacheable == true).ToArray();


                                    Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                    {
                                        StatusLabel.Content = "Pre-downloading station data.... Step 2 of 2.";
                                        StatusPB.Value = 0;
                                        StatusPB.Maximum = cacheAble.Length + 1;
                                    }));

                                    int prog = 1;
                                    foreach (var st in cacheAble)
                                    {
                                        var s = st;
                                        Hanasu.Services.Stations.StationsService.CheckAndDownloadCacheableStation(ref s);

                                        Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                        {
                                            StatusLabel.Content = "Pre-downloading station data.... Step 2 of 2.";
                                            StatusPB.Value = prog;
                                            StatusPB.Maximum = cacheAble.Length + 1;
                                        }));

                                        Thread.Sleep(700);

                                        prog++;
                                    }

                                    Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                    {
                                        WizardTabControl.SelectedIndex++;
                                    }));
                                }));
                        }
                        break;
                    }
                case 2:
                    {
                        NextBtn.IsEnabled = true;
                        break;
                    }
            }
        }
    }
}
