using System;
using Telemetry_Auth_API.Models;

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
        public Guid? WorkspaceId {get;set;}
        public Guid OwnerId {get;set;}
        public InstanceResponseDto(Instance instance)
        {
            Id = instance.Id;
            Name = instance.Name;
            WorkspaceId = instance.WorkspaceId;
            OwnerId = instance.OwnerId;
        }
    }
}