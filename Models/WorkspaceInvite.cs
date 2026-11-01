using System;
using System.Runtime.InteropServices;

namespace Telemetry_Auth_API.Models
{
    public class WorkspaceInvite
    {
        public Guid Id {get;set;}
        public Guid WorkspaceId {get; set;}
        public Workspace Workspace {get;set;} = null!;
        public string InviteCode {get;set;} = string.Empty;
        public WorkspaceRole Role {get;set;}
        public DateTime ExpiresAt {get;set;}
        public Guid CreatedById {get;set;}
    }
}