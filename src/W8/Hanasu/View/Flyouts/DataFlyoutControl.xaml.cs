using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Hanasu.View.Flyouts
{
    public sealed partial class DataFlyoutControl : UserControl
    {
        public DataFlyoutControl()
        {
            this.InitializeComponent();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Hanasu.App.AppFolder.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
            var files = await Hanasu.App.AppFolder.GetFilesAsync();
            foreach (var file in files)
                await file.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
            ((Button)sender).IsEnabled = false;
        }
    }
}
