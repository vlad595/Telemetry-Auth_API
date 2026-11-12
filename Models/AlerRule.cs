using System;

namespace Telemetry_Auth_API.Models
{
    public class AlertRules
    {
        public Guid Id {get;set;}
        public Guid UserId {get;set;}
        public User? User {get;set;} = null!;
        public string? TelegramChatId {get;set;}
        public List<string>? Rules {get;set;}
    }
}