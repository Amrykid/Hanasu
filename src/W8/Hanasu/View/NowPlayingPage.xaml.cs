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
                    CurrentSongTextBlock.Margin = new Thickness(40, 5, 0, 0);
                    CurrentSongTextBlock.TextAlignment = TextAlignment.Left;
                    CurrentSongTextBlock.Width = double.NaN;
                    pageTitle.Style = App.Current.Resources["PageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["BackButtonStyle"] as Style;
                    NowPlayingGrid.Width = 500;
                    PageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    NowPlayingHeader.Margin = new Thickness(50, 0, 0, 0);
                    MediaControlPanel.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                    MediaControlPanel.Margin = new Thickness(40, 0, 0, 0);
                    break;

                case "Snapped":
                    SongHistoryGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    StationImage.Width = 250;
                    StationImage.Height = 250;
                    StationImage.Margin = new Thickness(30, 15, 0, 0);
                    CurrentSongTextBlock.Margin = new Thickness(5, 15, 0, 0);
                    CurrentSongTextBlock.Width = 200;
                    CurrentSongTextBlock.TextWrapping = TextWrapping.Wrap;
                    CurrentSongTextBlock.TextAlignment = TextAlignment.Center;
                    pageTitle.Style = App.Current.Resources["SnappedPageHeaderTextStyle"] as Style;
                    backButton.Style = App.Current.Resources["SnappedBackButtonStyle"] as Style;
                    NowPlayingGrid.Width = 300;
                    PageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    MediaControlPanel.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                    MediaControlPanel.Margin = new Thickness(0, 0, 0, 0);
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

        private void HomeAppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
