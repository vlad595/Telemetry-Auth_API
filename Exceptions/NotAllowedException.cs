using System;

namespace Telemetry_Auth_API.Exceptions
{
    public class NotAllowedException: Exception
    {
        public NotAllowedException(): base() {}
        public NotAllowedException(string message): base(message) {}
    }
}