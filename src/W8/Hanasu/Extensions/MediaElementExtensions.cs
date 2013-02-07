using System;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Hanasu.Extensions
{
    //Converted and modified from http://blogs.msdn.com/b/lucian/archive/2012/11/28/how-to-await-a-mediaelement.aspx
    //May be buggy

    public static class MediaElementExtensions
    {
        //I modified the PlayAsync into a StopAsync method ~Amrykid
        public static Task PauseAsync(this MediaElement media, CancellationToken cancel)
        {

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (cancel.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            if (media.CurrentState == MediaElementState.Stopped)
            {
                tcs.SetException(new Exception("MediaElement is already closed."));
                return tcs.Task;
            }

            RoutedEventHandler lambdaEnded = null;
            RoutedEventHandler lambdaChanged = null;
            CancellationTokenRegistration? cancelReg = null;
            Action removeLambdas = () =>
            {
                media.MediaEnded -= lambdaEnded;
                media.CurrentStateChanged -= lambdaChanged;
                if (cancelReg.HasValue)
                    cancelReg.Value.Dispose();
            };

            lambdaEnded = (x, y) =>
            {
                removeLambdas();
                tcs.TrySetResult(null);
            };
            lambdaChanged = async (x, y) =>
            {
                if (media.CurrentState == MediaElementState.Stopped || media.CurrentState == MediaElementState.Paused)
                {
                    await Task.Delay(1000);

                    return;
                }
                removeLambdas();
                //tcs.TrySetCanceled();
            };

            media.MediaEnded += lambdaEnded;
            media.CurrentStateChanged += lambdaChanged;

            if (media.CurrentState == MediaElementState.Playing)
                media.Pause();

            //media.Stop();
            media.Pause();

            if (!tcs.Task.IsCompleted)
            {
                cancelReg = cancel.Register(() =>
                {
                    dynamic dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { media.Stop(); });
                });

            }

            return tcs.Task;
        }

        public static Task PlayAsync(this MediaElement media, CancellationToken cancel)
        {

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (cancel.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            if (media.CurrentState != MediaElementState.Paused)
            {
                tcs.SetException(new Exception("MediaElement not ready to play"));
                return tcs.Task;
            }

            RoutedEventHandler lambdaEnded = null;
            RoutedEventHandler lambdaChanged = null;
            CancellationTokenRegistration? cancelReg = null;
            Action removeLambdas = () =>
            {
                media.MediaEnded -= lambdaEnded;
                media.CurrentStateChanged -= lambdaChanged;
                if (cancelReg.HasValue)
                    cancelReg.Value.Dispose();
            };

            lambdaEnded = (x,y) =>
            {
                removeLambdas();
                tcs.TrySetResult(null);
            };
            lambdaChanged = (x,y) =>
            {
                if (media.CurrentState != MediaElementState.Stopped)
                    return;
                removeLambdas();
                tcs.TrySetCanceled();
            };

            media.MediaEnded += lambdaEnded;
            media.CurrentStateChanged += lambdaChanged;

            media.Play();

            if (!tcs.Task.IsCompleted)
            {
                cancelReg = cancel.Register(() =>
                {
                    dynamic dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { media.Stop(); });
                });

            }

            return tcs.Task;
        }

        public static Task OpenAsync(this MediaElement media, Uri uri, CancellationToken cancel)
        {

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            if (cancel.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            if (media.CurrentState == MediaElementState.Buffering || media.CurrentState == MediaElementState.Opening || media.CurrentState == MediaElementState.Playing)
            {
                tcs.SetException(new Exception("MediaElement not ready to open"));
                return tcs.Task;
            }

            RoutedEventHandler lambdaOpened = null;
            RoutedEventHandler lambdaChanged = null;
            ExceptionRoutedEventHandler lambdaFailed = null;
            CancellationTokenRegistration? cancelReg = null;
            Action removeLambdas = () =>
            {
                media.MediaOpened -= lambdaOpened;
                media.MediaFailed -= lambdaFailed;
                media.CurrentStateChanged -= lambdaChanged;
                if (cancelReg.HasValue)
                    cancelReg.Value.Dispose();
                //removeLambdas = () => return;
                // in case two lambdas get fired one after the other
            };

            lambdaOpened = (x,y) =>
            {
                removeLambdas();
                tcs.TrySetResult(null);
            };
            lambdaFailed = (s, e) =>
            {
                removeLambdas();
                tcs.TrySetException(new Exception(e.ErrorMessage));
            };
            lambdaChanged = (x,y) =>
            {
                if (media.CurrentState != MediaElementState.Closed)
                    return;
                removeLambdas();
                tcs.TrySetCanceled();
            };

            media.MediaOpened += lambdaOpened;
            media.MediaFailed += lambdaFailed;
            media.CurrentStateChanged += lambdaChanged;

            media.Source = uri;

            if (!tcs.Task.IsCompleted)
            {
                // The above condition guards against lambas being invoked by Source assignment
                cancelReg = cancel.Register(() =>
                {
                    dynamic dummy = media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { media.ClearValue(MediaElement.SourceProperty); });
                });
            }

            return tcs.Task;
        }

    }

}
