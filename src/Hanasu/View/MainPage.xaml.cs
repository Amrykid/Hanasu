using Hanasu.Model;
using Hanasu.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hanasu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public override void OnVisualStateChange(string newVisualState)
        {
            //layout aware page doesn't wanna work correctly... or its having trouble finding the views so im doing it manually

            switch (newVisualState)
            {
                case "Filled":
                case "FullScreenLandscape":
                case "FullScreenPortrait":
                    itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    itemListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    nowPlayingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    backButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    itemListView.Margin = new Thickness(0, 0, 0, 0);

                    foreach (Button ui in MediaControlPanel.Children)
                        ui.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case "Snapped":
                    itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    itemListView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    nowPlayingPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    backButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    itemListView.Margin = new Thickness(0, 0, 0, 20);

                    foreach (Button ui in MediaControlPanel.Children)
                        ui.Margin = new Thickness(-15, 0, -15, 0);
                    break;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var vm = ((MainPageViewModel)this.DataContext);
            var stat = (Station)e.ClickedItem;

            Task.Run(() => Dispatcher.ProcessEvents(Windows.UI.Core.CoreProcessEventsOption.ProcessAllIfPresent));

            await Task.Run(() => Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => vm.PlayStation(stat, globalMediaElement)));
        }
    }
}
