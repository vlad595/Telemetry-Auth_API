using System;

namespace Telemetry_Auth_API.DTO
{
    public class InstanceCreationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? WorkspaceId { get; set; }
    }
    public class InstanceResponseDto
    {
        public Guid Id {get;set;}
        public string Name {get;set;} = string.Empty;
        public Guid WorkspaceId {get;set;}
        public Guid OwnerId {get;set;}
    }
}