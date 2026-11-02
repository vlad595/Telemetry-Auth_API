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
    [Route("api/workspace")]
    [Authorize]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceService _service;
        public WorkspaceController(IWorkspaceService service)
        {
            _service = service;
        }
        [HttpGet("my")]
        public async Task<ActionResult<List<WorkspaceResponseDTO>>> MyWorkspaces()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.GetMyWorkspaces(Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound("Workspaces does not found");
            }catch(NotAllowedException ex)
            {
                return BadRequest();
            }
        }
        [HttpPost("")]
        public async Task<ActionResult<WorkspaceResponseDTO>> PostWorkspace(WorkspaceCreationDTO workspace)
        {
            try {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.AddWorkspace(workspace, Guid.Parse(userId));
                return Ok(response);
            } catch(NotFoundException ex)
            {
                return NotFound("Workspace does not found");
            } catch(NotAllowedException ex)
            {
                return BadRequest();
            }
        }
        [HttpDelete("remove={workspaceId}")]
        public async Task<ActionResult<WorkspaceResponseDTO>> RemoveWorkspace(string workspaceId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.RemoveWorkspace(Guid.Parse(workspaceId), Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound("Workspace does not found");
            }catch(NotAllowedException ex)
            {
                return BadRequest();
            }
        }
        [HttpPost("accept-invite={code}")]
        public async Task<ActionResult<WorkspaceResponseDTO>> AcceptInvite(string code)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _service.AcceptWorkspaceInvite(code, Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound("Workspace does not found");
            }catch(NotAllowedException ex)
            {
                return BadRequest();
            }catch(RequestTimeoutException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}