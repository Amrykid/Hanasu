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
    /// Interaction logic for PLSStreamChooseWindow.xaml
    /// </summary>
    public partial class MultiStreamChooseWindow : MetroWindow
    {
        public MultiStreamChooseWindow()
        {
            InitializeComponent();
            this.Unloaded += MultiStreamChooseWindow_Unloaded;
            this.Loaded += MultiStreamChooseWindow_Loaded;
        }

        void MultiStreamChooseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
        }

        void MultiStreamChooseWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.listBox1.MouseLeftButtonUp -= this.listBox1_MouseLeftButtonUp;
            this.Unloaded -= MultiStreamChooseWindow_Unloaded;
            this.Loaded -= MultiStreamChooseWindow_Loaded;

        }

        private void listBox1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listBox1.SelectedItem != null)
                this.DialogResult = true;
        }
    }
}
