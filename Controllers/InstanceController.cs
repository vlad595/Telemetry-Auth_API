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
    [Route("api/instance")]
    [Authorize]
    public class InstanceController: Controller
    {
        private readonly IInstanceService _service;
        public InstanceController(IInstanceService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<ActionResult<InstanceResponseDto>> AddInstance(InstanceCreationDto instance)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.AddInstance(instance, Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("workspace")]
        public async Task<ActionResult<InstanceResponseDto>> AddInstanceToWorkspace(WorkspaceInstanceCreationDto instance)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.AddInstanceToWorkspace(instance, Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<InstanceResponseDto>> RemoveInstance(string instanceId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.RemoveInstance(Guid.Parse(instanceId), Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("my")]
        public async Task<ActionResult<List<InstanceResponseDto>>> GetMyInstances()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.GetMyInstances(Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("workspace={workspaceId}")]
        public async Task<ActionResult<List<InstanceResponseDto>>> GetWorkspaceInstances(string workspaceId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.GetWorkspaceInstances(Guid.Parse(workspaceId), Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}