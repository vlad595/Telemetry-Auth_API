using System;
using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Npgsql.Internal;
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

        public Task<InstanceResponseDto> TransferInstanceBetweenWorkspaces(Guid instanceId, Guid toWorkspace, Guid fromWorkspace, Guid userId);
        public Task<InstanceResponseDto> TransferInstanceBetweenOwners(Guid instanceId, Guid toNewOwner, Guid currentOwner);
        public Task<InstanceResponseDto> TransferInstanceFromOwnerToWorkspace(Guid instanceId, Guid workspaceId, Guid ownerId);

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
            var workspace = await _context.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
            {
                throw new NotFoundException("Workspace not found");
            }

            bool isOwner = workspace.OwnerId == userId;
            var user = workspace.Members.FirstOrDefault(u => u.UserId == userId);

            if (!isOwner && user == null)
            {
                throw new NotAllowedException("You are not allowed to see other workspace instances");
            }

            var workspaceInstances = await _context.Instances.Where(i => i.WorkspaceId == workspaceId).ToListAsync();
            if (!workspaceInstances.Any()) 
            {
                throw new NotFoundException("There are no instances");
            }

            return workspaceInstances.Select(i => new InstanceResponseDto(i)).ToList();
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

        public async Task<InstanceResponseDto> TransferInstanceBetweenWorkspaces(Guid instanceId, Guid toWorkspace, Guid fromWorkspace, Guid userId)
        {
            var instance = await _context.Instances.FirstOrDefaultAsync(i => i.Id == instanceId);
            if (instance == null)
            {
                throw new NotFoundException("Instance does not found");
            }

            if (instance.WorkspaceId != fromWorkspace || instance.OwnerId != userId || fromWorkspace == toWorkspace)
            {
                throw new NotAllowedException("You cannot transfer this instance from the specified workspace");
            }

            var workspace = await _context.Workspaces.AsNoTracking().Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == toWorkspace);
            if (workspace == null)
            {
                throw new NotFoundException("Worksapce does not found");
            }

            var member = workspace.Members.FirstOrDefault(m => m.UserId == userId);
            if (workspace.OwnerId != userId && (member == null || member.UserRole >= WorkspaceRole.Reader))
            {
                throw new NotAllowedException("You are not allowed to do this action");
            }

            instance.WorkspaceId = toWorkspace;
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(instance);
        }
        public async Task<InstanceResponseDto> TransferInstanceBetweenOwners(Guid instanceId, Guid toNewOwner, Guid currentOwner)
        {
            var instance = await _context.Instances.FirstOrDefaultAsync(i => i.Id == instanceId);
            if (instance == null)
            {
                throw new NotFoundException("Instance does not found");
            }
            if (instance.OwnerId != currentOwner)
            {
                throw new NotAllowedException("You are not the owner of thit instance");
            }

            if (!await _context.Users.AnyAsync(user => user.Id == toNewOwner))
            {
                throw new NotFoundException("New owner does not found");
            }

            instance.OwnerId = toNewOwner;
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(instance);
        }
        public async Task<InstanceResponseDto> TransferInstanceFromOwnerToWorkspace(Guid instanceId, Guid workspaceId, Guid ownerId)
        {
            var instance = await _context.Instances.FirstOrDefaultAsync(i => i.Id == instanceId);
            if (instance == null)
            {
                throw new NotFoundException("Instance does not found");
            }
            if (instance.OwnerId != ownerId)
            {
                throw new NotAllowedException("You are not the owner of thit instance");
            }

            var workspace = await _context.Workspaces.AsNoTracking().Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("Worksapce does not found");
            }
            var member = workspace.Members.FirstOrDefault(m => m.UserId == ownerId);
            if (workspace.OwnerId != ownerId && (member == null || member.UserRole >= WorkspaceRole.Reader))
            {
                throw new NotAllowedException("You are not allowed to do this action");
            }
            if (instance.WorkspaceId.HasValue)
            {
                throw new NotAllowedException("Instance already in other workspace. Try transfer instance between workspaces");
            }

            instance.WorkspaceId = workspaceId;
            await _context.SaveChangesAsync();
            return new InstanceResponseDto(instance);
        }
    }
}
