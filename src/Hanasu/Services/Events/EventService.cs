using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hanasu.Services.Events
{
    public class EventService : IStaticService
    {
        static EventService()
        {
            if (EventHandlers == null)
                Initialize();
        }

        public static void Initialize()
        {
            EventHandlers = new Collection<EventReference>();
        }
        private static Collection<EventReference> EventHandlers = null;
        public static EventReference AttachHandler(EventType type, Action<EventInfo> act)
        {
            var eref = new EventReference(type, act);

            EventHandlers.Add(eref);

            return eref;
        }

        public static void DetachHandler(EventReference eref)
        {
            EventHandlers.Remove(eref);
        }

        public static void RaiseEventAsync(EventType type, EventInfo data)
        {
            Task.Factory.StartNew(() =>
            {
                RaiseEvent(type, data);
            }).ContinueWith(t => t.Dispose());
        }
        public static void RaiseEvent(EventType type, ref EventInfo data)
        {
            RaiseEvent(type, data);
        }
        public static void RaiseEvent(EventType type, EventInfo data)
        {
            data.EventType = type;

            foreach (EventReference eref in EventHandlers.Where(rf => rf.EventType == type))
                eref.HandlerMethod.Invoke(data);
        }
    }
}
