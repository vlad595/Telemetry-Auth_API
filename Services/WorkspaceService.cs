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
        public Task<WorkspaceResponseDTO> AddWorkspace(WorkspaceCreationDTO workspace);
        public Task<WorkspaceResponseDTO> RemoveWorkspace(Guid workspaceId, Guid userId);
    }
    public class WorkspaceService: IWorkspaceService
    
    {
        private readonly AppDbContext _context;
        public WorkspaceService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<WorkspaceResponseDTO> AddWorkspace(WorkspaceCreationDTO workspace)
        {
            Workspace newWorkspace = new Workspace
            {
                Id = Guid.NewGuid(),
                WorkspaceName = workspace.WorkspaceName,
                OwnerId = workspace.OwnerId
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
    }
}