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
        public Task<WorkspaceResponseDTO> AddWorkspace(WorkspaceCreationDTO workspace, Guid userId);
        public Task<WorkspaceResponseDTO> RemoveWorkspace(Guid workspaceId, Guid userId);
        public Task<WorkspaceResponseDTO> AcceptWorkspaceInvite(string code, Guid userId);
    }
    public class WorkspaceService: IWorkspaceService

    {
        private readonly AppDbContext _context;
        public WorkspaceService(AppDbContext context)
        {
            _context = context;
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
        public async Task<WorkspaceResponseDTO> AcceptWorkspaceInvite(string code, Guid userId)
        {
            var invite = await _context.Invites.FirstOrDefaultAsync(i => i.InviteCode == code);
            if (invite == null)
            {
                throw new NotFoundException("There is no invite by that code");
            }
            else if (invite.ExpiresAt < DateTime.Now)
            {
                throw new RequestTimeoutException("Time for accepting this invite is out");
            }
            var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == invite.WorkspaceId);
            if (workspace == null)
            {
                throw new NotFoundException("There is no workspace by this invite.");
            }
            workspace.Members.Add(new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserRole = invite.Role,
                UserId = userId
            });
            
            _context.Invites.Remove(invite);
            
            await _context.SaveChangesAsync();
            return new WorkspaceResponseDTO(workspace);
        }
        public async Task<List<WorkspaceResponseDTO>> GetMyWorkspaces(Guid userId)
        {
            var workspaces = await _context.Workspaces.Where(w => w.Members.Any(m => m.UserId == userId)).ToListAsync();
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