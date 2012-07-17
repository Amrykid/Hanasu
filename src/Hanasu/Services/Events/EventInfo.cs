using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Events
{
    public abstract class EventInfo
    {
        public EventType EventType { get; set; }
        public static EventInfo Empty { get { return new EmptyEventInfo(); } }
    }
     class EmptyEventInfo : EventInfo
    {
    }
}
