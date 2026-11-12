using System;

namespace Telemetry_Auth_API.Models
{
    public enum Subscription
    {
        Tier1 = 0,
        Tier2 = 1,
        Tier3 = 2
    }
    public enum WorkspaceRole
    {
        Admin,
        Manager,
        Reader,
    }
    public class User
    {
        public Guid Id {get;set;}
        public string UserName {get;set;} = string.Empty;
        public string Email {get;set;} = string.Empty;
        public Subscription SubscriptionLevel {get;set;}
        public string PasswordHash {get;set;} = string.Empty;
    }
}