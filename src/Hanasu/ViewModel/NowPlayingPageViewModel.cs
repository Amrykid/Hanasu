using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanasu.ViewModel
{
    public class NowPlayingPageViewModel: BaseViewModel
    {
        public override void OnNavigatedFrom()
        {
            
        }
        public override void OnNavigatedTo(dynamic argument = null)
        {
            //grab any arguments pass to the page when it was navigated to.

            if (argument == null) return;

            var args = (KeyValuePair<string, string>)argument[0];
        }
    }
}
