using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Hanasu.ViewModel
{
    public class NowPlayingPageViewModel: Crystal.Dynamic.AutoIPNotifyingBaseViewModel
    {
        public override void OnNavigatedFrom()
        {
            
        }
        public override void OnNavigatedTo(dynamic argument = null)
        {
            //grab any arguments pass to the page when it was navigated to.

            if (argument == null) return;

            var args = (KeyValuePair<string, string>)argument[0];

            CurrentStation = ((App)App.Current).AvailableStations.First(x => x.Title == args.Value);

            Image = CurrentStation.Image;
            RaisePropertyChanged(x => this.Image);


        }

        public Station CurrentStation { get; set; }

        public ImageSource Image { get; set; }
    }
}
