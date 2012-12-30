using Crystal.Messaging;
using Crystal.Navigation;
using Hanasu.Model;
using Hanasu.ViewModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hanasu
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.

    [Crystal.Navigation.NavigationSetViewModel(typeof(SearchPageViewModel))]
    public sealed partial class SearchPage : LayoutAwarePage
    {
        public SearchPage()
        {
            this.InitializeComponent();
            this.Loaded += GroupPage_Loaded;
        }

        void GroupPage_Loaded(object sender, RoutedEventArgs e)
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
                    pageTitle.Style = App.Current.Resources["PageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["BackButtonStyle"] as Style;
                    break;

                case "Snapped":
                    pageTitle.Style = App.Current.Resources["SnappedPageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["SnappedBackButtonStyle"] as Style;
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter; //grab any paramaters that was passed for navigation.

            pageTitle.Text = ((SearchPageViewModel)this.DataContext).TitleString; //set the page title to the localized string

            //bind the page title to the localized string on the view model so it auto updates.
            Crystal.Binding.AutoUpdatePropertyHelper.BindObjects<SearchPageViewModel>(((SearchPageViewModel)this.DataContext), x => x.TitleString, pageTitle, TextBlock.TextProperty);

            base.OnNavigatedTo(e);
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //handles when an item is clicked

            var vm = ((SearchPageViewModel)this.DataContext);
            var stat = (Station)e.ClickedItem; //grab the clicked station

            Messenger.PushMessage(vm, "PlayStation", stat); //sends a message to the main view model to play the station

            //await Task.Run(() => Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => vm.PlayStation(stat, globalMediaElement)));

            //NavigationService.NavigateToAsHome<MainPageViewModel>();

            if (NavigationService.CanGoBack)
                NavigationService.GoBack(); //goes back to the main page.
            else
                NavigationService.NavigateTo<MainPageViewModel>(new KeyValuePair<string, string>("StationToPlay", stat.Title)); //in rare cases, go straight to the main page, passying the station to play, with it.
        }
    }
}
