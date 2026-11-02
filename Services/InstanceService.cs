using System;
using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Services
{
    public interface IInstanceService
    {
        public Task<InstanceResponseDto> AddInstance(InstanceCreationDto instance, Guid userId);
        public Task<InstanceResponseDto> AddInstanceToWorkspace(WorkspaceInstanceCreationDto instance, Guid userId);
        public Task<InstanceResponseDto> RemoveInstance(Guid instanceId, Guid userId);
        public Task<List<InstanceResponseDto>> GetWorkspaceInstances(Guid workspaceId, Guid userId);
        public Task<List<InstanceResponseDto>> GetMyInstances(Guid userId);
    }
    public class InstanceService: IInstanceService
    {
        private readonly AppDbContext _context;
        public InstanceService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<InstanceResponseDto> AddInstance(InstanceCreationDto instance, Guid userId)
        {
            Instance newInstance = new Instance
            {
                Id = instance.Id,
                OwnerId = userId,
                Name = instance.Name
            };

            _context.Instances.Add(newInstance);
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(newInstance);
        }
        public async Task<InstanceResponseDto> AddInstanceToWorkspace(WorkspaceInstanceCreationDto instance, Guid userId)
        {
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == instance.WorkspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("There is no workspace");
            }
            var user = workspace.Members.FirstOrDefault(u => u.UserId == userId);
            if (workspace.OwnerId != userId && (user == null || user.UserRole >= WorkspaceRole.Reader))
            {
                throw new NotAllowedException("You are not allowed to add instances");
            }

            Instance newInstance = new Instance
            {
                Id = instance.Id,
                OwnerId = userId,
                Name = instance.Name,
                WorkspaceId = instance.WorkspaceId
            };

            _context.Instances.Add(newInstance);
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(newInstance);
        }
        public async Task<InstanceResponseDto> RemoveInstance(Guid instanceId, Guid userId)
        {
            var instance = await _context.Instances.FirstOrDefaultAsync(i => i.Id == instanceId);

            if (instance == null)
            {
                throw new NotFoundException();
            }

            else if (instance.OwnerId != userId)
            {
                throw new NotAllowedException("You are not the owner or the admin of a workspace.");
            }

            _context.Instances.Remove(instance);
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(instance);
        }
        public async Task<List<InstanceResponseDto>> GetWorkspaceInstances(Guid workspaceId, Guid userId)
        {
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("Workspace does not found");
            }
            var user = workspace.Members.FirstOrDefault(u => u.UserId == userId);
            if (workspace.OwnerId != userId && (user == null || user.UserRole >= WorkspaceRole.Reader))
            {
                throw new NotAllowedException("You are not allowed to see other workspace instances");
            }

            List<Instance> workspaceInstances = await _context.Instances.Where(i => i.WorkspaceId == workspaceId).ToListAsync();

            if (workspaceInstances == null)
            {
                throw new NotFoundException("There is no instances");
            }

            List<InstanceResponseDto> result = new();
            foreach (Instance instance in workspaceInstances)
            {
                result.Add(new InstanceResponseDto(instance));
            }
            return result;
        }
        public async Task<List<InstanceResponseDto>> GetMyInstances(Guid userId)
        {
            List<Instance> workspaceInstances = await _context.Instances.Where(i => i.OwnerId == userId).ToListAsync();

            if (workspaceInstances == null)
            {
                throw new NotFoundException();
            }

            List<InstanceResponseDto> result = new();
            foreach (Instance instance in workspaceInstances)
            {
                result.Add(new InstanceResponseDto(instance));
            }
            return result;
        }
    }
}