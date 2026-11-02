using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Services;

namespace Telemetry_Auth_API.Controllers
{
    [ApiController]
    [Route("api/invites")]
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly IInvitesService _service;
        public InvitesController(IInvitesService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<ActionResult<InviteResponseDto>> CreateInvite(InviteCreationDto invite)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.CreateInvite(invite, Guid.Parse(userId));
                return Ok(response);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<InviteResponseDto>> RemoveInvite(string inviteId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.RemoveInvite(Guid.Parse(inviteId), Guid.Parse(userId));
                return Ok(response);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }catch(NotFoundException ex)
            {
                return NotFound();
            }
        }
    }
}