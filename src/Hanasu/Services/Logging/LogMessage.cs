using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Logging
{
    public sealed class LogMessage
    {
        public Type Sender { get; set; }
        public string Message { get; set; }
    }
}
