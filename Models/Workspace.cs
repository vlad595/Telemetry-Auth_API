using System;

namespace Telemetry_Auth_API.Models
{
    public class Workspace
    {
        public Guid Id {get;set;}
        public ICollection<WorkspaceMember> Members {get;set;} = new List<WorkspaceMember>();
        public Guid OwnerId {get;set;}
        public User Owner {get;set;} = null!;
        public ICollection<Instance> Instances {get;set;} = new List<Instance>();
    }
}