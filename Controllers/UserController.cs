using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Services;
using Telemetry_Auth_API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Telemetry_Auth_API.Controllers
{
    [ApiController]
    [Route("/api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _service;
        private readonly IAlertRulesService _alertRulesService;
        public UserController(IUserService service, IAlertRulesService alertRulesService)
        {
            _service = service;
            _alertRulesService = alertRulesService;
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
        [HttpGet("instance={agentId}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserByInstanceId(string agentId)
        {
            try
            {
                var response = await _service.GetPlanByAgentId(Guid.Parse(agentId));
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
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

        [Tags("Telegram")]
        [HttpGet("telegram/agent={agentId}")]
        public async Task<ActionResult<InstanceTelegramRecipientsDto>> GetUsersChatIdByAgentId(string agentId)
        {
            try
            {
                var response = await _alertRulesService.GetUsersTelegramIdByAgentId(Guid.Parse(agentId));
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Tags("Telegram")]
        [HttpGet("telegram/user={userId}")]
        public async Task<ActionResult<AlertRuleResponseDto>> GetUserTelegramChatId(string userId)
        {
            try
            {
                var response = await _alertRulesService.GetUserTelegramChatId(Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Tags("Telegram")]
        [HttpPost("telegram")]
        [Authorize]
        public async Task<ActionResult<string>> GenerateTelegramCode()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NotFoundException("User id does not found");
                var response = await _alertRulesService.MakeTelegramCode(Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Tags("Telegram")]
        [HttpPatch("telegram/create-connection")]
        public async Task<ActionResult<AlertRuleResponseDto>> CreateUserConnection(string code, string chatId)
        {
            try
            {
                var response = await _alertRulesService.StoreTelegramChatId(code, chatId);
                return Ok(response);
            }catch (NotFoundException ex)
            {
               return NotFound(ex.Message); 
            }catch (ArgumentException ex)
            {
               return BadRequest(ex.Message);
            }
        }
        [Tags("Telegram")]
        [HttpDelete("telegram/delete-connection")]
        [Authorize]
        public async Task<ActionResult<AlertRuleResponseDto>> RemoveConnection()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NotFoundException("User id does not found");
                var response = await _alertRulesService.RemoveTelegramChatId(Guid.Parse(userId));
                return Ok(response);
            }catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
