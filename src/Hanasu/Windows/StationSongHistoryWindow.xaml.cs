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
using System.Collections;
using MahApps.Metro.Controls;

namespace Hanasu.Windows
{
    /// <summary>
    /// Interaction logic for StationSongHistoryWindow.xaml
    /// </summary>
    public partial class StationSongHistoryWindow : MetroWindow
    {
        public StationSongHistoryWindow()
        {
            InitializeComponent();
            this.Unloaded += StationAttributesWindow_Unloaded;
        }

        void StationAttributesWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.@this.Loaded -= new System.Windows.RoutedEventHandler(this.MetroWindow_Loaded);
            this.Unloaded -= StationAttributesWindow_Unloaded;

            BindingOperations.ClearAllBindings(this);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hanasu.Services.Settings.SettingsThemeHelper.ApplyThemeAccordingToSettings(this);
            DG.ItemsSource = (System.Collections.Generic.Dictionary<System.String,System.String>)this.DataContext;
        }
    }
}
