using System;

namespace Telemetry_Auth_API.Models
{
    public class WorkspaceMember
    {
        public Guid WorkspaceId {get;set;}
        public Workspace Workspace {get;set;} = null!;
        public Guid UserId {get;set;}
        public User User {get;set;} = null!;
        public WorkspaceRole UserRole {get;set;}
    }
}