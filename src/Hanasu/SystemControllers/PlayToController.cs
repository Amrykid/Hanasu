using Crystal.Messaging;
using Hanasu.Tools.Shoutcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.PlayTo;
using Windows.UI.Xaml.Controls;

namespace Hanasu.SystemControllers
{
    public static class PlayToController
    {
        public static void Initialize(MediaElement me)
        {
            if (((App)App.Current).ptm == null)
            {
                ((App)App.Current).ptm = Windows.Media.PlayTo.PlayToManager.GetForCurrentView();
                ((App)App.Current).ptm.SourceRequested += ptm_SourceRequested;
                ((App)App.Current).ptm.SourceSelected += ptm_SourceSelected;

                globalMediaElement = me;
            }
        }

        public static PlayToSourceSelectedEventArgs CurrentConnectionDetails { get; private set; }

        static void ptm_SourceSelected(Windows.Media.PlayTo.PlayToManager sender, Windows.Media.PlayTo.PlayToSourceSelectedEventArgs args)
        {
            CurrentConnectionDetails = args;

            Crystal.Dispatcher.DispatcherService.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
               {
                   globalMediaElement.PlayToSource.Connection.StateChanged += Connection_StateChanged;
                   globalMediaElement.PlayToSource.Connection.Transferred += Connection_Transferred;
                   globalMediaElement.PlayToSource.Connection.Error += Connection_Error;
               });
        }

        static void Connection_Error(Windows.Media.PlayTo.PlayToConnection sender, Windows.Media.PlayTo.PlayToConnectionErrorEventArgs args)
        {

        }

        static void Connection_Transferred(Windows.Media.PlayTo.PlayToConnection sender, Windows.Media.PlayTo.PlayToConnectionTransferredEventArgs args)
        {
            throw new NotImplementedException();
        }

        static void Connection_StateChanged(Windows.Media.PlayTo.PlayToConnection sender, Windows.Media.PlayTo.PlayToConnectionStateChangedEventArgs args)
        {
            IsConnectedViaPlayTo = args.CurrentState != Windows.Media.PlayTo.PlayToConnectionState.Disconnected;

            if (PlayToConnectionStateChanged != null)
                PlayToConnectionStateChanged();

            if (!IsConnectedViaPlayTo)
                CurrentConnectionDetails = null;
        }

        public delegate void PlayToConnectionStateChangedHandler();
        public static event PlayToConnectionStateChangedHandler PlayToConnectionStateChanged;

        private static MediaElement globalMediaElement = null;

        static void ptm_SourceRequested(Windows.Media.PlayTo.PlayToManager sender, Windows.Media.PlayTo.PlayToSourceRequestedEventArgs args)
        {
            //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh465191.aspx

            Windows.Media.PlayTo.PlayToSourceRequest sr = args.SourceRequest;
            Windows.Media.PlayTo.PlayToSource controller = null;
            Windows.Media.PlayTo.PlayToSourceDeferral deferral = args.SourceRequest.GetDeferral();

            Crystal.Dispatcher.DispatcherService.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                controller = ((MediaElement)globalMediaElement).PlayToSource;

                if (!await IsStreamable(globalMediaElement.Source))
                {
                    sr.DisplayErrorString("This is station is not streamable to devices.");
                }
                else
                    sr.SetSource(controller);

                deferral.Complete();
            });

            //args.SourceRequest.SetSource(globalMediaElement.PlayToSource);
        }

        private static async Task<bool> IsStreamable(Uri uri)
        {
            return await Messenger.
        }


        public static bool IsConnectedViaPlayTo { get; private set; }
    }
}
