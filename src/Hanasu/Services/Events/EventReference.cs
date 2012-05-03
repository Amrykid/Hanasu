using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Events
{
    public class EventReference
    {
        internal EventReference(EventType type, Action<EventInfo> ei)
        {
            HandlerMethod = ei;
            EventType = type;
        }
        internal Action<EventInfo> HandlerMethod { get; set; }
        public EventType EventType { get; internal set; }
    }
}
