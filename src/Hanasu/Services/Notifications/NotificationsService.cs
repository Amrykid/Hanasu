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
using Hanasu.Services.Logging;
using System.Collections.Concurrent;

namespace Hanasu.Services.Notifications
{
    public class NotificationsService : IStaticService
    {
        static NotificationsService()
        {
            Notifications = new ConcurrentQueue<NotificationInfo>();

            LogService.Instance.WriteLog(typeof(NotificationsService),
    "Notifications Service initialized.");
        }

        public static ConcurrentQueue<NotificationInfo> Notifications { get; private set; }
        public static bool QueueRunning { get; private set; }

        public static void ClearNotificationQueue()
        {
            lock (Notifications)
            {
                Queue<NotificationInfo> tmp = new Queue<NotificationInfo>();

                while (!Notifications.IsEmpty)
                {
                    NotificationInfo d = null;
                    if (Notifications.TryPeek(out d))
                    {

                        if (d.IsUrgent)
                            tmp.Enqueue(d);

                        Notifications.TryDequeue(out d);
                    }
                }

                foreach (NotificationInfo ni in tmp)
                    Notifications.Enqueue(ni);

                LogService.Instance.WriteLog(typeof(NotificationsService),
    "Notifications queue cleared.");
            }
        }


        public static void AddNotification(string title, string message = "", int duration = 3000, bool isUrgent = false, NotificationType type = NotificationType.Information, Action<NotificationInfo> onClickCallback = null)
        {
            if (System.Windows.Application.Current == null)
                return;

            if (title == null)
                throw new ArgumentNullException("title");
            if (message == null)
                throw new ArgumentNullException("message");
            if (duration == 0)
                throw new ArgumentOutOfRangeException("duration");

            lock (Notifications)
            {
                Notifications.Enqueue(
                    new NotificationInfo()
                    {
                        Title = title,
                        Message = message,
                        Duration = duration,
                        IsUrgent = isUrgent,
                        Type = type,
                        OnClickCallback = onClickCallback
                    });

                LogService.Instance.WriteLog(typeof(NotificationsService),
    "Notification added.");
            }

            HandleQueue();
        }

        static void HandleQueue()
        {
            if (QueueRunning)
                return;

            ThreadPool.QueueUserWorkItem(new WaitCallback(t =>
                {

                    while (!Notifications.IsEmpty)
                    {
                        if (System.Windows.Application.Current == null)
                        {
                            QueueRunning = false;
                            return;
                        }

                        QueueRunning = true;
                        try
                        {
                            NotificationsWindow nw = null;
                            NotificationInfo msg = null;

                            if (Notifications.TryDequeue(out msg))
                            {
                                Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                                    () =>
                                    {
                                        nw = new NotificationsWindow();

                                        nw.DataContext = msg;

                                        nw.Show();


                                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                                while (nw.IsVisible)
                                    Thread.Sleep(50);

                                Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                                    () =>
                                    {
                                        nw.Close();
                                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }


                            LogService.Instance.WriteLog(typeof(NotificationsService),
    "Notification shown: " + ((dynamic)nw.DataContext).Title);
                        }
                        catch (Exception)
                        {
                            if (Application.Current == null)
                                System.Diagnostics.Process.GetCurrentProcess().Kill(); //If by some rare bug, Hanasu doesn't exit, force itself to die.
                        }
                    }

                    QueueRunning = false;
                }));
        }

        public delegate void EmptyDelegate();
        public delegate object EmptyReturnDelegate();

    }
}
