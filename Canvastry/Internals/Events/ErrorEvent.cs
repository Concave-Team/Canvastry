using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.Internals.Events
{
    public class ErrorEvent : Event {}
    public class ErrorEventData : EventData
    {
        public string Message;
        public string Severity;
        public string Location;

        public ErrorEventData(object sender, string message, string severity, string location) : base(sender)
        {
            Message = message;
            Severity = severity;
            Location = location;
        }
    }
}
