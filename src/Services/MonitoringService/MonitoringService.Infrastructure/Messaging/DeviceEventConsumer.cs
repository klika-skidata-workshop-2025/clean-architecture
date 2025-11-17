using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitoringService.Application.Common.Interfaces;
using MonitoringService.Domain.Entities;
using MonitoringService.Domain.Enums;
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Configuration;
using Workshop.Messaging.Events;
using Workshop.Messaging.Implementation;

namespace MonitoringService.Infrastructure.Messaging;

/// <summary>
/// Consumes device events from RabbitMQ and evaluates monitoring rules.
/// </summary>
public class DeviceEventConsumer : RabbitMQConsumerBase
{
    protected override string QueueName => "monitoring.device-events";
    protected override string[] RoutingKeys => new[] { "device.#" };

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceEventConsumer> _logger;

    public DeviceEventConsumer(
        IServiceProvider serviceProvider,
        IOptions<RabbitMQOptions> options,
        ILogger<DeviceEventConsumer> logger)
        : base(options, logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(string message, string routingKey, CancellationToken cancellationToken)
    {
        try
        {
            // Try to deserialize as DeviceStatusChangedEvent
            var statusEvent = DeserializeMessage<DeviceStatusChangedEvent>(message);
            if (statusEvent != null)
            {
                await HandleDeviceStatusChangedAsync(statusEvent, cancellationToken);
                return;
            }

            // Try to deserialize as DeviceHeartbeatEvent
            var heartbeatEvent = DeserializeMessage<DeviceHeartbeatEvent>(message);
            if (heartbeatEvent != null)
            {
                await HandleDeviceHeartbeatAsync(heartbeatEvent, cancellationToken);
                return;
            }

            _logger.LogWarning("Unknown device event type received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing device event");
        }
    }

    private async Task HandleDeviceStatusChangedAsync(
        DeviceStatusChangedEvent evt,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing DeviceStatusChanged event: {DeviceId} - {Status}",
            evt.DeviceId, evt.NewStatus);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();

        // Get all enabled monitoring rules that apply to this device
        var rules = await context.MonitoringRules
            .Where(r => r.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var rule in rules)
        {
            // Check if rule applies to this device
            if (!rule.AppliesTo(evt.DeviceId, evt.DeviceType))
                continue;

            // Evaluate rule condition
            bool triggered = EvaluateRule(rule, evt);

            if (triggered)
            {
                _logger.LogInformation("Monitoring rule triggered: {RuleName} for device: {DeviceId}",
                    rule.Name, evt.DeviceId);

                // Create alert
                var alert = Alert.Create(
                    title: $"{rule.Name} - {evt.DeviceName}",
                    message: $"Device {evt.DeviceName} ({evt.DeviceType}) changed status from {evt.OldStatus} to {evt.NewStatus} at {evt.Location}",
                    severity: rule.AlertSeverity,
                    deviceId: evt.DeviceId,
                    deviceName: evt.DeviceName,
                    ruleId: rule.Id,
                    ruleName: rule.Name);

                context.Alerts.Add(alert);

                // Record that rule was triggered
                rule.RecordTrigger();

                // Publish alert triggered event
                await publisher.PublishAsync(new AlertTriggeredEvent
                {
                    AlertId = alert.Id,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    DeviceId = evt.DeviceId,
                    DeviceName = evt.DeviceName,
                    Severity = rule.AlertSeverity.ToString(),
                    Message = alert.Message
                }, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleDeviceHeartbeatAsync(
        DeviceHeartbeatEvent evt,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing DeviceHeartbeat event: {DeviceId}", evt.DeviceId);

        // Extract additional data from HealthData dictionary
        evt.HealthData.TryGetValue("DeviceName", out var deviceName);
        evt.HealthData.TryGetValue("EventType", out var eventType);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();

        // Get all enabled monitoring rules that apply to this device
        var rules = await context.MonitoringRules
            .Where(r => r.IsEnabled)
            .Where(r => r.ConditionType == RuleConditionType.DeviceEventTypeMatches)
            .ToListAsync(cancellationToken);

        foreach (var rule in rules)
        {
            // Check if rule applies to this device
            if (!rule.AppliesTo(evt.DeviceId, evt.DeviceType))
                continue;

            // Check if event type matches condition
            if (rule.ConditionValue == eventType || rule.ConditionValue == "*")
            {
                _logger.LogInformation("Monitoring rule triggered by heartbeat: {RuleName} for device: {DeviceId}",
                    rule.Name, evt.DeviceId);

                // Create alert
                var alert = Alert.Create(
                    title: $"{rule.Name} - {deviceName ?? evt.DeviceId}",
                    message: $"Device {deviceName ?? evt.DeviceId} triggered event: {eventType}",
                    severity: rule.AlertSeverity,
                    deviceId: evt.DeviceId,
                    deviceName: deviceName ?? evt.DeviceId,
                    ruleId: rule.Id,
                    ruleName: rule.Name);

                context.Alerts.Add(alert);

                // Record that rule was triggered
                rule.RecordTrigger();

                // Publish alert triggered event
                await publisher.PublishAsync(new AlertTriggeredEvent
                {
                    AlertId = alert.Id,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    DeviceId = evt.DeviceId,
                    DeviceName = deviceName ?? evt.DeviceId,
                    Severity = rule.AlertSeverity.ToString(),
                    Message = alert.Message
                }, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private bool EvaluateRule(MonitoringRule rule, DeviceStatusChangedEvent evt)
    {
        return rule.ConditionType switch
        {
            RuleConditionType.DeviceStatusEquals =>
                evt.NewStatus.Equals(rule.ConditionValue, StringComparison.OrdinalIgnoreCase),

            RuleConditionType.DeviceOfflineForDuration =>
                evt.NewStatus.Equals("Offline", StringComparison.OrdinalIgnoreCase),

            _ => false
        };
    }
}
