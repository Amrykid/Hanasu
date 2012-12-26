using Hanasu.Model;
using Hanasu.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    
    [Crystal.Navigation.NavigationSetViewModel(typeof(GroupPageViewModel))]
    public sealed partial class GroupPage : LayoutAwarePage
    {
        public GroupPage()
        {
            this.InitializeComponent();
            this.Loaded += GroupPage_Loaded;
        }

        async void GroupPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public override void OnVisualStateChange(string newVisualState)
        {
            //layout aware page doesn't wanna work correctly... or its having trouble finding the views so im doing it manually

            switch (newVisualState)
            {
                case "Filled":
                case "FullScreenLandscape":
                case "FullScreenPortrait":
                    break;

                case "Snapped":
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter;

            pageTitle.Text = ((GroupPageViewModel)this.DataContext).GroupName;

            base.OnNavigatedTo(e);
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var vm = ((GroupPageViewModel)this.DataContext);
            var stat = (Station)e.ClickedItem;

            Task.Run(() => Dispatcher.ProcessEvents(Windows.UI.Core.CoreProcessEventsOption.ProcessAllIfPresent));

            //await Task.Run(() => Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => vm.PlayStation(stat, globalMediaElement)));
        }
    }
}
