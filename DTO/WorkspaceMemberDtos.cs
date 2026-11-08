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
        public Guid UserId {get;set;}
        public string UserName {get;set;} = string.Empty;
        public WorkspaceRole UserRole {get;set;}
        public WorkspaceMemberResponseDTO(WorkspaceMember member)
        {
            UserId = member.UserId;
            UserRole = member.UserRole;
            UserName = string.IsNullOrWhiteSpace(member.UserName)
                ? member.User?.UserName ?? string.Empty
                : member.UserName;
        }
    }
}
