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

    [Crystal.Navigation.NavigationSetViewModel(typeof(NowPlayingPageViewModel))]
    public sealed partial class NowPlayingPage : LayoutAwarePage
    {
        public NowPlayingPage()
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
                    SongHistoryGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    StationImage.Margin = new Thickness(-100, 20, 0, 0);
                    StationImage.Width = 300;
                    StationImage.Height = 300;
                    break;

                case "Snapped":
                    SongHistoryGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    StationImage.Width = 200;
                    StationImage.Height = 200;
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter;

            //pageTitle.Text = ((SearchPageViewModel)this.DataContext).TitleString;

            //Crystal.Binding.AutoUpdatePropertyHelper.BindObjects<SearchPageViewModel>(((SearchPageViewModel)this.DataContext), x => x.TitleString, pageTitle, TextBlock.TextProperty);

            base.OnNavigatedTo(e);
        }
    }
}
