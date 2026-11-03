using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Services;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Controllers
{
    [ApiController]
    [Route("/api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _service;
        public UserController(IUserService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDTO>> Me()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Invalid token");
            }
            var response = await _service.Me(Guid.Parse(userId));
            return Ok(response);
        }
        [HttpPost("reg")]
        public async Task<ActionResult<UserAuthResponseDTO>> Register(UserRegDTO user)
        {
            var response = await _service.Register(user);
            return Ok(response);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserAuthResponseDTO>> LogIn(UserLogInDTO user)
        {
            try
            {
                var response = await _service.LogIn(user);
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(NotAllowedException ex)
            {
                return BadRequest($"Password is incorrect: {ex.Message}");
            }
        }
        [HttpPatch("plan/downgrade={plan}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> DowngradePlan(Subscription plan)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.DowngradePlan(Guid.Parse(userId), plan);
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound($"User does not found: {ex.Message}");
            }catch(NotAllowedException ex)
            {
                return BadRequest($"Something went wrong: {ex.Message}");
            }
        }
        [HttpPatch("plan/upgrade={plan}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> UpgradePlan(Subscription plan)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.UpgradePlan(Guid.Parse(userId), plan);
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound($"User does not found: {ex.Message}");
            }catch(NotAllowedException ex)
            {
                return BadRequest($"Something went wrong: {ex.Message}");
            }
        }
        [HttpDelete("remove={userId}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> DeleteUser(string userId)
        {
            try
            {
                string tokenUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "No id";
                var response = await _service.DeleteUser(Guid.Parse(userId), Guid.Parse(tokenUserId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound($"User does not found: {ex.Message}");
            }catch(NotAllowedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
