using System;

namespace Telemetry_Auth_API.Models
{
    public class Instance
    {
        public Guid Id {get;set;}
        public string Name {get;set;} = string.Empty;
        public Guid OwnerId {get;set;}
        public User Owner {get;set;} = null!;
        public Guid? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }
    }
}