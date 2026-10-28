using System;

namespace Telemetry_Auth_API.Models
{
    public enum Subscription
    {
        Tier1,
        Tier2,
        Tier3
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
        public ICollection<Instance> Instances {get;set;} = new List<Instance>();
        public ICollection<WorkspaceMember> WorkspaceMembership {get;set;} = new List<WorkspaceMember>();
        public string PasswordHash {get;set;} = string.Empty;
    }
}