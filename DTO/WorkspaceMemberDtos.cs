using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class WorkspaceMemberCreationDTO
    {
        public Guid WorkspaceId {get;set;}
        public Guid UserId {get;set;}
        public WorkspaceRole UserRole {get;set;}
    }
    public class WorkspaceMemberResponseDTO
    {
        public Guid WorkspaceId {get;set;}
        public Guid UserId {get;set;}
        public WorkspaceRole UserRole {get;set;}
    }
}