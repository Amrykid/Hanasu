using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crystal.Core;
using Crystal.Localization;
using Hanasu.Model;
using Hanasu.Extensions;

namespace Hanasu.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public override async void OnNavigatedTo(KeyValuePair<string, object>[] argument = null)
        {
            RegisterForMessages("UpdateIsBusy");

            NowPlayingPaneViewModel = new NowPlayingViewModel();
            StationsPaneViewModel = new StationsViewModel();
            HomeTabViewModel = new HomeViewModel();

            base.OnNavigatedTo(argument);
        }

        public override bool ReceiveMessage(object source, Crystal.Messaging.Message message)
        {
            switch (message.MessageString.ToLower())
            {
                case "updateisbusy":
                    UIDispatcher.BeginInvoke(new Action(() =>
                        {
                            IsBusy = (bool)message.Data;
                        }));
                    return true;
            }

            return base.ReceiveMessage(source, message);
        }

        public NowPlayingViewModel NowPlayingPaneViewModel
        {
            get { return GetPropertyOrDefaultType<NowPlayingViewModel>(x => this.NowPlayingPaneViewModel); }
            set { SetProperty(x => this.NowPlayingPaneViewModel, value); }
        }

        public StationsViewModel StationsPaneViewModel
        {
            get { return GetPropertyOrDefaultType<StationsViewModel>(x => this.StationsPaneViewModel); }
            set { SetProperty(x => this.StationsPaneViewModel, value); }
        }
        public HomeViewModel HomeTabViewModel
        {
            get { return GetPropertyOrDefaultType<HomeViewModel>(x => this.HomeTabViewModel); }
            set { SetProperty(x => this.HomeTabViewModel, value); }
        }

        [Crystal.Messaging.PropertyMessage("IsBusy")]
        public bool IsBusy
        {
            get { return GetPropertyOrDefaultType<bool>(x => this.IsBusy); }
            set { SetProperty(x => this.IsBusy, value); }
        }
    }
}
