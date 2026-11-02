using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class WorkspaceCreationDTO
    {
        public string WorkspaceName {get;set;} = string.Empty;
    }
    public class WorkspaceResponseDTO
    {
        public Guid Id {get;set;}
        public string WorkspaceName {get;set;} = string.Empty;
        public ICollection<WorkspaceMember> Members {get;set;} = new List<WorkspaceMember>();
        public Guid OwnerId {get;set;}
        public WorkspaceResponseDTO(Workspace workspace)
        {
            Id = workspace.Id;
            WorkspaceName = workspace.WorkspaceName;
            OwnerId = workspace.OwnerId;
            Members = workspace.Members;
        }
    }
}