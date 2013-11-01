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
            NowPlayingPaneViewModel = new NowPlayingViewModel();
            StationsPaneViewModel = new StationsViewModel();

            base.OnNavigatedTo(argument);
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
    }
}
