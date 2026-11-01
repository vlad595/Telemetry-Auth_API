using System;

namespace Telemetry_Auth_API.Exceptions
{
    public class RequestTimeoutException : Exception
    {
        public RequestTimeoutException() : base(){}
        public RequestTimeoutException(string message) : base(message){}
    }
}