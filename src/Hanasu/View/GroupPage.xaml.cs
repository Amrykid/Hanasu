using Crystal.Messaging;
using Crystal.Navigation;
using Hanasu.Model;
using Hanasu.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var vm = ((GroupPageViewModel)this.DataContext);
            var stat = (Station)e.ClickedItem;

            NavigationService.GoBack();

            Messenger.PushMessage(vm, "PlayStation", stat);

            //await Task.Run(() => Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => vm.PlayStation(stat, globalMediaElement)));
        }
    }
}
