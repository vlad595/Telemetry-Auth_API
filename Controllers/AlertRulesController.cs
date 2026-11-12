using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Services;

namespace Telemetry_Auth_API.Controllers;

[ApiController]
[Authorize]
[Route("api/alert-rules")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRulesService _service;

    public AlertRulesController(IAlertRulesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetRules()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.GetRules(userId)); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPost]
    public async Task<ActionResult<AlertRuleResponseDto>> AddRule(AlertRuleRequestDto request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.AddRule(request.Rule, userId)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpDelete]
    public async Task<ActionResult<AlertRuleResponseDto>> DeleteRule(AlertRuleRequestDto request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try { return Ok(await _service.DeleteRule(request.Rule, userId)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (NotFoundException ex) { return NotFound(ex.Message); }
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
