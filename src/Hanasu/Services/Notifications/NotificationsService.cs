using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Media;
using Hanasu.Services.Notifications;
using Hanasu.Core;

namespace Hanasu.Services.Notifications
{
    public class NotificationsService
    {
        static NotificationsService()
        {
            Notifications = new ObservableQueue<NotificationInfo>();
        }

        public static ObservableQueue<NotificationInfo> Notifications { get; private set; }
        public static bool QueueRunning { get; private set; }

        public static void AddNotification(string title, string message, int duration)
        {
            lock (Notifications)
            {
                Notifications.Enqueue(
                    new NotificationInfo()
                    {
                        Title = title,
                        Message = message,
                        Duration = duration
                    });
            }

            HandleQueue();
        }

        static void HandleQueue()
        {
            if (QueueRunning)
                return;

            Thread t = new Thread(() =>
                {
                    while (!Notifications.IsEmpty)
                    {
                        QueueRunning = true;
                        try
                        {
                            NotificationsWindow nw = null;
                            Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                                () =>
                                {
                                    nw = new NotificationsWindow();
                                    nw.DataContext = Notifications.Dequeue();

                                    nw.Show();

                                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            while (nw.IsVisible)
                                Thread.Sleep(50);
                        }
                        catch (Exception) { }
                    }

                    QueueRunning = false;
                });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public delegate void EmptyDelegate();
    }
}
