using System;
using Microsoft.EntityFrameworkCore;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Services
{
    public interface IWorkspaceService
    {
        public Task<List<WorkspaceResponseDTO>> GetMyWorkspaces(Guid userId);
        public Task<WorkspaceResponseDTO> GetWorkspace(Guid workspaceId, Guid userId);
        public Task<WorkspaceResponseDTO> AddWorkspace(WorkspaceCreationDTO workspace, Guid userId);
        public Task<WorkspaceResponseDTO> RemoveWorkspace(Guid workspaceId, Guid userId);
        public Task<WorkspaceResponseDTO> ChangeMemberRole(Guid workspaceId, Guid userId, Guid memberId, WorkspaceRole role);
        public Task<WorkspaceResponseDTO> RemoveMember(Guid workspaceId, Guid memberId, Guid userId);
        public Task<WorkspaceResponseDTO> AcceptWorkspaceInvite(string code, Guid userId);
    }
    public class WorkspaceService: IWorkspaceService

    {
        private readonly AppDbContext _context;
        public WorkspaceService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<WorkspaceResponseDTO> GetWorkspace(Guid workspaceId, Guid userId)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(member => member.User)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
            {
                throw new NotFoundException();
            }

            List<WorkspaceMember> members = workspace.Members.ToList();

            if (workspace.OwnerId != userId && !members.Any(m => m.UserId == userId))
            {
                throw new NotAllowedException("You are not the member of a workspace.");
            }
            return new WorkspaceResponseDTO(workspace);
        }
        public async Task<WorkspaceResponseDTO> AddWorkspace(WorkspaceCreationDTO workspace, Guid userId)
        {
            Workspace newWorkspace = new Workspace
            {
                Id = Guid.NewGuid(),
                WorkspaceName = workspace.WorkspaceName,
                OwnerId = userId
            };

            _context.Workspaces.Add(newWorkspace);
            await _context.SaveChangesAsync();
            return new WorkspaceResponseDTO(newWorkspace);
        }
        public async Task<WorkspaceResponseDTO> RemoveWorkspace(Guid workspaceId, Guid userId)
        {
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
            {
                throw new NotFoundException();
            }
            else if (workspace.OwnerId != userId)
            {
                throw new NotAllowedException("You are not the owner of a workspace.");
            }

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            return new WorkspaceResponseDTO(workspace);
        }
        public async Task<WorkspaceResponseDTO> ChangeMemberRole(Guid workspaceId, Guid userId, Guid memberId, WorkspaceRole role)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(member => member.User)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("Workspace not found.");
            }

            bool isOwner = workspace.OwnerId == userId;
            var currentUser = workspace.Members.FirstOrDefault(m => m.UserId == userId);

            if (!isOwner && currentUser == null)
            {
                throw new NotAllowedException("You are not a member of this workspace.");
            }

            var memberToChange = workspace.Members.FirstOrDefault(m => m.UserId == memberId);
            if (memberToChange == null)
            {
                throw new NotFoundException("Member not found in this workspace.");
            }
            if (memberToChange.UserId == workspace.OwnerId)
            {
                throw new NotAllowedException("Cannot change the role of the workspace owner.");
            }

            if (!isOwner && currentUser!.UserRole != WorkspaceRole.Admin)
            {
                throw new NotAllowedException("You are not allowed to change member roles.");
            }
            memberToChange.UserRole = role; 

            await _context.SaveChangesAsync();
            return new WorkspaceResponseDTO(workspace);
        }
        public async Task<WorkspaceResponseDTO> RemoveMember(Guid workspaceId, Guid memberId, Guid userId)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(member => member.User)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
            {
                throw new NotFoundException("Workspace not found");
            }

            bool isOwner = workspace.OwnerId == userId;
            var user = workspace.Members.FirstOrDefault(m => m.UserId == userId);

            if (!isOwner && user == null)
            {
                throw new NotAllowedException("You are not a member of this workspace.");
            }

            var memberToRemove = workspace.Members.FirstOrDefault(m => m.UserId == memberId);

            if (memberToRemove == null)
            {
                throw new NotFoundException("Member not found in this workspace");
            }

            if (!isOwner && user!.UserRole != WorkspaceRole.Admin && user!.UserRole != WorkspaceRole.Manager)
            {
                throw new NotAllowedException("You cannot remove members.");
            }

            workspace.Members.Remove(memberToRemove);

            await _context.SaveChangesAsync();

            return new WorkspaceResponseDTO(workspace);

        }
        public async Task<WorkspaceResponseDTO> AcceptWorkspaceInvite(string code, Guid userId)
        {
            var invite = await _context.Invites.FirstOrDefaultAsync(i => i.InviteCode == code);
            if (invite == null)
            {
                throw new NotFoundException("There is no invite by that code");
            }
            else if (invite.ExpiresAt < DateTime.UtcNow)
            {
                throw new RequestTimeoutException("Time for accepting this invite is out");
            }
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(member => member.User)
                .FirstOrDefaultAsync(w => w.Id == invite.WorkspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("There is no workspace by this invite.");
            }
            workspace.Members.Add(new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserRole = invite.Role,
                UserId = userId,
                UserName = await _context.Users
                    .Where(user => user.Id == userId)
                    .Select(user => user.UserName)
                    .FirstOrDefaultAsync() ?? string.Empty
            });
            
            _context.Invites.Remove(invite);
            
            await _context.SaveChangesAsync();
            return new WorkspaceResponseDTO(workspace);
        }
        public async Task<List<WorkspaceResponseDTO>> GetMyWorkspaces(Guid userId)
        {
            var workspaces = await _context.Workspaces
                .Where(w => w.Members.Any(m => m.UserId == userId) || w.OwnerId == userId)
                .Include(w => w.Members)
                    .ThenInclude(member => member.User)
                .ToListAsync();
            if (workspaces == null)
            {
                throw new NotFoundException();
            }

            List<WorkspaceResponseDTO> response = new();

            foreach(var workspace in workspaces)
            {
                response.Add(new WorkspaceResponseDTO(workspace));    
            }

            return response;
        }
    }
}
