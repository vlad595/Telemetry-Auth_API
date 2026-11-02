using System;
using Microsoft.EntityFrameworkCore;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Services
{
    public interface IInvitesService
    {
        public Task<InviteResponseDto> CreateInvite(InviteCreationDto invite, Guid userId);
        public Task<InviteResponseDto> RemoveInvite(Guid inviteId, Guid userId);
    }
    public class InvitesService : IInvitesService
    {
        private readonly AppDbContext _context;
        public InvitesService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<InviteResponseDto> CreateInvite(InviteCreationDto invite, Guid userId)
        {
            WorkspaceInvite newInvite = new WorkspaceInvite
            {
                Id = Guid.NewGuid(),
                WorkspaceId = invite.WorkspaceId,
                Role = invite.Role,
                InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                CreatedById = userId
            };

            _context.Invites.Add(newInvite);
            await _context.SaveChangesAsync();

            return new InviteResponseDto(newInvite);
        }
        public async Task<InviteResponseDto> RemoveInvite(Guid inviteId, Guid userId)
        {
            var invite = await _context.Invites.FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null)
            {
                throw new NotFoundException();
            }
            else if (invite.CreatedById != userId)
            {
                throw new NotAllowedException();
            }
            
            _context.Remove(invite);
            await _context.SaveChangesAsync();
            return new InviteResponseDto(invite);
        }
    }
} 