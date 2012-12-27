using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Hanasu.SystemControllers
{
    public static class PlayToController
    {
        public static void Initialize(ref MediaElement me)
        {
            if (((App)App.Current).ptm == null)
            {
                ((App)App.Current).ptm = Windows.Media.PlayTo.PlayToManager.GetForCurrentView();
                ((App)App.Current).ptm.SourceRequested += ptm_SourceRequested;

                globalMediaElement = me;
            }
        }
        private static MediaElement globalMediaElement = null;

        static async void ptm_SourceRequested(Windows.Media.PlayTo.PlayToManager sender, Windows.Media.PlayTo.PlayToSourceRequestedEventArgs args)
        {
            //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh465191.aspx

            await Crystal.Dispatcher.DispatcherService.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                    Windows.Media.PlayTo.PlayToSourceRequest sr = args.SourceRequest;
                    Windows.Media.PlayTo.PlayToSource controller = null;
                    Windows.Media.PlayTo.PlayToSourceDeferral deferral = args.SourceRequest.GetDeferral();
                    controller = ((MediaElement)globalMediaElement).PlayToSource;
                    
                    sr.SetSource(controller);
                    deferral.Complete();
            });
        }

    }
}
