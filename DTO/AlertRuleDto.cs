using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.DTO
{
    public class AlertRuleRequestDto
    {
        public string Rule { get; set; } = string.Empty;
    }

    public class AlertRuleResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? TelegramChatId { get; set; }
        public List<string> Rules { get; set; }

        public AlertRuleResponseDto(AlertRules alertRules)
        {
            ArgumentNullException.ThrowIfNull(alertRules);

            Id = alertRules.Id;
            UserId = alertRules.UserId;
            TelegramChatId = alertRules.TelegramChatId;
            Rules = alertRules.Rules?.ToList() ?? [];
        }
    }

    public class InstanceTelegramRecipientsDto
    {
        public Guid InstanceId { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public string? InstanceWorkspace { get; set; }
        public Dictionary<Guid, TelegramRecipientDto> Users { get; set; } = [];
    }

    public class TelegramRecipientDto
    {
        public string UserName { get; set; } = string.Empty;
        public string UserChatId { get; set; } = string.Empty;
    }

    public class AlertRuleConditionDto
    {
        public string FirstField { get; set; } = string.Empty;
        public string ComparisonOperator { get; set; } = string.Empty;
        public string SecondField { get; set; } = string.Empty;
    }
}
