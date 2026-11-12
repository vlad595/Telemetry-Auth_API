using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Telemetry_Auth_API.Data;
using Telemetry_Auth_API.DTO;
using Telemetry_Auth_API.Exceptions;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Services;

public interface IAlertRulesService
{
    Task<InstanceTelegramRecipientsDto> GetUsersTelegramIdByAgentId(Guid agentId);
    Task<AlertRuleResponseDto> GetUserTelegramChatId(Guid userId);
    Task<string> MakeTelegramCode(Guid userId);
    Task<AlertRuleResponseDto> StoreTelegramChatId(string code, string chatId);
    Task<AlertRuleResponseDto> RemoveTelegramChatId(Guid userId);
    Task<AlertRuleResponseDto> AddRule(string rule, Guid userId);
    Task<AlertRuleResponseDto> DeleteRule(string rule, Guid userId);
    Task<List<string>> GetRules(Guid userId);
}

public class AlertRulesService : IAlertRulesService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public AlertRulesService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<InstanceTelegramRecipientsDto> GetUsersTelegramIdByAgentId(Guid agentId)
    {
        var instance = await _context.Instances.AsNoTracking().Include(i => i.Owner).Include(i => i.Workspace)
            .ThenInclude(w => w!.Owner).Include(i => i.Workspace).ThenInclude(w => w!.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(i => i.Id == agentId) ?? throw new NotFoundException("Agent does not found");

        var users = (instance.Workspace == null ? new[] { instance.Owner } : instance.Workspace.Members.Select(m => m.User)
                .Append(instance.Workspace.Owner).DistinctBy(u => u.Id).ToArray())
            .Where(u => u.SubscriptionLevel is Subscription.Tier1 or Subscription.Tier2)
            .ToArray();
        
        var userIds = users.Select(u => u.Id).ToArray();
        
        var recipients = await _context.AlertRules.AsNoTracking()
            .Where(a => userIds.Contains(a.UserId) && a.TelegramChatId != null && a.TelegramChatId != "")
            .Select(a => new { a.UserId, a.TelegramChatId }).ToListAsync();
        
        var names = users.ToDictionary(u => u.Id, u => u.UserName);

        return new InstanceTelegramRecipientsDto
        {
            InstanceId = instance.Id,
            InstanceName = instance.Name,
            InstanceWorkspace = instance.Workspace?.WorkspaceName,
            Users = recipients.ToDictionary(a => a.UserId, a => new TelegramRecipientDto
            {
                UserName = names[a.UserId],
                UserChatId = a.TelegramChatId!
            })
        };
    }

    public async Task<AlertRuleResponseDto> GetUserTelegramChatId(Guid userId)
    {
        await EnsureUserExists(userId);
        var alertRules = await _context.AlertRules.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId);
        if (string.IsNullOrWhiteSpace(alertRules?.TelegramChatId))
            throw new NotFoundException("User does not have connected Telegram");
        return new AlertRuleResponseDto(alertRules);
    }

    public async Task<string> MakeTelegramCode(Guid userId)
    {
        await EnsureUserExists(userId);
        if (_cache.TryGetValue($"userId:{userId}", out string? code) && code != null)
            return $"https://t.me/Opentelemetrybot?start={code}";

        code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        _cache.Set($"userId:{userId}", code, TimeSpan.FromMinutes(10));
        _cache.Set(code, userId, TimeSpan.FromMinutes(10));
        return $"https://t.me/Opentelemetrybot?start={code}";
    }

    public async Task<AlertRuleResponseDto> StoreTelegramChatId(string code, string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("Telegram chat ID is required", nameof(chatId));
        if (string.IsNullOrWhiteSpace(code) || !_cache.TryGetValue(code, out Guid userId))
            throw new NotFoundException("Telegram link code is invalid or expired");

        await EnsureUserExists(userId);
        var alertRules = await GetOrCreateRules(userId);
        alertRules.TelegramChatId = chatId.Trim();
        await _context.SaveChangesAsync();
        _cache.Remove(code);
        _cache.Remove($"userId:{userId}");
        return new AlertRuleResponseDto(alertRules);
    }

    public async Task<AlertRuleResponseDto> RemoveTelegramChatId(Guid userId)
    {
        await EnsureUserExists(userId);
        var alertRules = await GetOrCreateRules(userId);
        alertRules.TelegramChatId = null;
        await _context.SaveChangesAsync();
        return new AlertRuleResponseDto(alertRules);
    }

    public async Task<AlertRuleResponseDto> AddRule(string rule, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(rule))
            throw new ArgumentException("Rule is required", nameof(rule));
        await EnsureUserExists(userId);
        var alertRules = await GetOrCreateRules(userId);
        alertRules.Rules ??= [];
        var normalizedRule = rule.Trim();
        if (!alertRules.Rules.Contains(normalizedRule, StringComparer.Ordinal))
        {
            alertRules.Rules.Add(normalizedRule);
            await _context.SaveChangesAsync();
        }
        return new AlertRuleResponseDto(alertRules);
    }

    public async Task<AlertRuleResponseDto> DeleteRule(string rule, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(rule))
            throw new ArgumentException("Rule is required", nameof(rule));
        await EnsureUserExists(userId);
        var alertRules = await _context.AlertRules.FirstOrDefaultAsync(a => a.UserId == userId);
        if (alertRules?.Rules == null || !alertRules.Rules.Remove(rule.Trim()))
            throw new NotFoundException("Rule does not exist");
        await _context.SaveChangesAsync();
        return new AlertRuleResponseDto(alertRules);
    }

    public async Task<List<string>> GetRules(Guid userId)
    {
        await EnsureUserExists(userId);
        var alertRules = await _context.AlertRules.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId);
        return alertRules?.Rules?.ToList() ?? [];
    }

    private async Task EnsureUserExists(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId))
            throw new NotFoundException("User does not found");
    }

    private async Task<AlertRules> GetOrCreateRules(Guid userId)
    {
        var alertRules = await _context.AlertRules.FirstOrDefaultAsync(a => a.UserId == userId);
        if (alertRules != null)
            return alertRules;

        alertRules = new AlertRules { Id = Guid.NewGuid(), UserId = userId, Rules = [] };
        _context.AlertRules.Add(alertRules);
        return alertRules;
    }
}
