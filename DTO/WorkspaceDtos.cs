using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class WorkspaceCreationDTO
    {
        public Guid OwnerId {get;set;}
        public string WorkspaceName {get;set;} = string.Empty;
    }
    public class WorkspaceResponse
    {
        public Guid Id {get;set;}
        public string WorkspaceName {get;set;} = string.Empty;
        public ICollection<WorkspaceMember> Members {get;set;} = new List<WorkspaceMember>();
        public Guid OwnerId {get;set;}
        public ICollection<Instance> Instances {get;set;} = new List<Instance>();
    }
}