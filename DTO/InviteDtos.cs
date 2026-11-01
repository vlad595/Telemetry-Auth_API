using System;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class InviteCreationDto
    {
        public Guid WorkspaceId {get;set;}
        public WorkspaceRole Role {get;set;}
    }
    public class InviteResponseDto
    {
        public Guid WorkspaceId {get;set;}
        public string InviteCode {get;set;} = string.Empty;
        public WorkspaceRole Role {get;set;}
        public DateTime ExpiresAt {get;set;}
        public Guid CreatedById {get;set;}
        public InviteResponseDto(WorkspaceInvite invite)
        {
            WorkspaceId = invite.Id;
            InviteCode = invite.InviteCode;
            Role = invite.Role;
            ExpiresAt = invite.ExpiresAt;
            CreatedById = invite.CreatedById;
        }
    }
}