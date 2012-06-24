using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core;

namespace Hanasu.Services.Logging
{
    public class LogService : BaseINPC, IStaticService
    {
        static LogService()
        {
            if (Instance == null)
                Initialize();
        }
        public static void Initialize()
        {
            if (Instance == null)
                Instance = new LogService();
        }
        public static LogService Instance { get; private set; }

        private LogService()
        {
            Messages = new ObservableQueue<LogMessage>();
        }

        public ObservableQueue<LogMessage> Messages { get; private set; }

        public void WriteLog(object from, string msg)
        {
#if DEBUG
            WriteLog(from.GetType(),
                msg);
#endif
        }
        public void WriteLog(Type from, string msg)
        {
#if DEBUG
            if (System.Windows.Application.Current == null)
                return;

            Messages.Enqueue(
                new LogMessage()
                {
                    Sender = from,
                    Message = msg,
                    Time = DateTime.Now
                });

            OnPropertyChanged("Messages");
#endif
        }
    }
}
