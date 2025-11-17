# Workshop.Messaging

RabbitMQ messaging abstractions and event DTOs for the Skidata Workshop microservices.

## Overview

This library provides everything needed for event-driven communication between workshop services:

- **Publisher** - Publishes events to RabbitMQ topic exchange
- **Consumer Base Class** - Base class for creating event consumers
- **Event DTOs** - Strongly-typed event definitions
- **Configuration** - RabbitMQ connection settings

## Installation

```bash
dotnet add package Workshop.Messaging
```

## Configuration

Add RabbitMQ settings to `appsettings.json`:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "workshop",
    "Password": "workshop123",
    "VirtualHost": "/",
    "ExchangeName": "workshop.events",
    "ExchangeType": "topic",
    "Durable": true
  }
}
```

## Publishing Events

### 1. Register Publisher

In `Program.cs`:

```csharp
using Workshop.Messaging;

builder.Services.AddRabbitMQPublisher(builder.Configuration);
```

### 2. Inject and Use

```csharp
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Events;

public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, Result>
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly IApplicationDbContext _context;

    public UpdateDeviceCommandHandler(
        IRabbitMQPublisher publisher,
        IApplicationDbContext context)
    {
        _publisher = publisher;
        _context = context;
    }

    public async Task<Result> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.FindAsync(request.DeviceId);
        // ... update device

        // Publish event
        await _publisher.PublishAsync(new DeviceStatusChangedEvent
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            DeviceType = device.Type.ToString(),
            OldStatus = device.PreviousStatus,
            NewStatus = device.Status.ToString(),
            Reason = request.Reason,
            Location = device.Location
        }, cancellationToken);

        return Result.Ok();
    }
}
```

## Consuming Events

### 1. Create Consumer Class

Inherit from `RabbitMQConsumerBase`:

```csharp
using Workshop.Messaging.Implementation;
using Workshop.Messaging.Events;

public class DeviceStatusConsumer : RabbitMQConsumerBase
{
    private readonly ILogger<DeviceStatusConsumer> _logger;

    // Unique queue name for this consumer
    protected override string QueueName => "monitoring.device.status";

    // Routing keys to subscribe to (supports wildcards)
    protected override string[] RoutingKeys => new[]
    {
        "device.status.*",      // All device status events
        "device.heartbeat"      // Device heartbeat events
    };

    public DeviceStatusConsumer(
        IOptions<RabbitMQOptions> options,
        ILogger<DeviceStatusConsumer> logger)
        : base(options, logger)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        string message,
        string routingKey,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message on routing key: {RoutingKey}", routingKey);

        // Deserialize based on routing key
        if (routingKey == "device.status.changed")
        {
            var evt = DeserializeMessage<DeviceStatusChangedEvent>(message);

            // Process event
            if (evt.NewStatus == "BLOCKED")
            {
                _logger.LogWarning("Device {DeviceId} is BLOCKED!", evt.DeviceId);
                // Trigger alert...
            }
        }
        else if (routingKey == "device.heartbeat")
        {
            var evt = DeserializeMessage<DeviceHeartbeatEvent>(message);
            // Update last seen timestamp...
        }

        await Task.CompletedTask;
    }
}
```

### 2. Register Consumer

In `Program.cs`:

```csharp
// Register consumer as hosted service (runs in background)
builder.Services.AddHostedService<DeviceStatusConsumer>();
```

## Event Types

### DeviceStatusChangedEvent

Published when a device status changes.

**Routing Key:** `device.status.changed`

**Published By:** Device Service
**Consumed By:** Monitoring Service, Diagnostics Service

```csharp
await _publisher.PublishAsync(new DeviceStatusChangedEvent
{
    DeviceId = "gate-42",
    DeviceName = "Main Entrance Gate",
    DeviceType = "GATE",
    OldStatus = "ACTIVE",
    NewStatus = "BLOCKED",
    Reason = "Security alert",
    Location = "Building A"
});
```

### AlertTriggeredEvent

Published when a monitoring alert is triggered.

**Routing Key:** `monitoring.alert.triggered`

**Published By:** Monitoring Service
**Consumed By:** Diagnostics Service, Device Service

```csharp
await _publisher.PublishAsync(new AlertTriggeredEvent
{
    AlertId = "alert-123",
    DeviceId = "gate-42",
    DeviceName = "Main Entrance Gate",
    Severity = "CRITICAL",
    Title = "Device Blocked",
    Description = "Gate has been blocked due to security alert",
    RuleId = "rule-device-blocked",
    RuleName = "Device Blocked Rule"
});
```

### HealthCheckFailedEvent

Published when a health check fails.

**Routing Key:** `diagnostics.health.failed`

**Published By:** Diagnostics Service
**Consumed By:** Monitoring Service

```csharp
await _publisher.PublishAsync(new HealthCheckFailedEvent
{
    ComponentName = "DeviceService",
    ComponentType = "SERVICE",
    HealthStatus = "UNHEALTHY",
    Reason = "Service not responding",
    ErrorMessage = "Connection timeout after 30 seconds",
    IsCritical = true
});
```

### DeviceHeartbeatEvent

Published periodically by devices to indicate they are alive.

**Routing Key:** `device.heartbeat`

**Published By:** Device Service
**Consumed By:** Diagnostics Service

```csharp
await _publisher.PublishAsync(new DeviceHeartbeatEvent
{
    DeviceId = "gate-42",
    Status = "ACTIVE",
    DeviceType = "GATE",
    Location = "Building A",
    HealthData = new Dictionary<string, string>
    {
        { "uptime_seconds", "12345" },
        { "requests_per_minute", "42" }
    }
});
```

## Routing Key Patterns

RabbitMQ topic exchanges support wildcard patterns:

- `*` - Matches exactly one word
- `#` - Matches zero or more words

### Examples:

```csharp
// Subscribe to all device events
protected override string[] RoutingKeys => new[] { "device.*" };

// Subscribe to specific device status events
protected override string[] RoutingKeys => new[] { "device.status.changed" };

// Subscribe to all alert events
protected override string[] RoutingKeys => new[] { "monitoring.alert.*" };

// Subscribe to ALL events (use sparingly!)
protected override string[] RoutingKeys => new[] { "#" };

// Subscribe to multiple patterns
protected override string[] RoutingKeys => new[]
{
    "device.status.*",
    "monitoring.alert.*"
};
```

## Advanced Consumer Configuration

Override protected properties to customize consumer behavior:

```csharp
public class CustomConsumer : RabbitMQConsumerBase
{
    protected override string QueueName => "my.custom.queue";
    protected override string[] RoutingKeys => new[] { "device.*" };

    // Make queue non-durable (doesn't survive broker restarts)
    protected override bool IsDurable => false;

    // Make queue exclusive to this connection
    protected override bool IsExclusive => true;

    // Auto-delete queue when consumer disconnects
    protected override bool AutoDelete => true;

    // Prefetch 50 messages for better throughput
    protected override ushort PrefetchCount => 50;

    // ...
}
```

## Error Handling

### Publisher Errors

Publisher throws exceptions on:
- Null message
- RabbitMQ connection unavailable

```csharp
try
{
    await _publisher.PublishAsync(event);
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Failed to publish event - RabbitMQ unavailable");
    // Handle error (retry, dead letter queue, etc.)
}
```

### Consumer Errors

Consumers automatically:
- Nack and requeue messages that fail processing
- Log errors
- Continue processing other messages

Override `HandleMessageAsync` with try-catch for custom error handling:

```csharp
protected override async Task HandleMessageAsync(
    string message,
    string routingKey,
    CancellationToken cancellationToken)
{
    try
    {
        var evt = DeserializeMessage<DeviceStatusChangedEvent>(message);
        await ProcessEventAsync(evt);
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "Invalid message format - skipping");
        // Don't rethrow - message will be acknowledged and removed from queue
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing message");
        throw; // Rethrow - message will be nack'd and requeued
    }
}
```

## Testing

### Mock Publisher

```csharp
var mockPublisher = Substitute.For<IRabbitMQPublisher>();

// Setup
mockPublisher.IsConnected.Returns(true);

// Use in tests
var handler = new UpdateDeviceCommandHandler(mockPublisher, context);
await handler.Handle(command);

// Verify
await mockPublisher.Received(1).PublishAsync(
    Arg.Is<DeviceStatusChangedEvent>(e => e.DeviceId == "gate-42"));
```

### Integration Tests with Testcontainers

```csharp
using Testcontainers.RabbitMq;

public class RabbitMQIntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer _rabbitMqContainer;

    public async Task InitializeAsync()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();

        await _rabbitMqContainer.StartAsync();
    }

    [Fact]
    public async Task Publisher_Should_Publish_Event()
    {
        // Arrange
        var options = Options.Create(new RabbitMQOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672)
        });

        var publisher = new RabbitMQPublisher(options, logger);

        // Act
        await publisher.PublishAsync(new DeviceStatusChangedEvent
        {
            DeviceId = "test-device"
        });

        // Assert
        Assert.True(publisher.IsConnected);
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
    }
}
```

## Best Practices

### 1. Use Specific Routing Keys

```csharp
// Good - Specific and descriptive
public string Topic => "device.status.changed";

// Bad - Too generic
public string Topic => "event";
```

### 2. Include Metadata

```csharp
public class DeviceStatusChangedEvent : IMessage
{
    // ...
    public Dictionary<string, string> Metadata { get; set; } = new();
}

// Usage
event.Metadata["user_id"] = userId;
event.Metadata["correlation_id"] = correlationId;
```

### 3. Version Your Events

When breaking changes are needed, create a new event type:

```csharp
// V1
public class DeviceStatusChangedEvent : IMessage { }

// V2 (breaking changes)
public class DeviceStatusChangedEventV2 : IMessage
{
    public string Topic => "device.status.changed.v2";
}
```

### 4. Use Idempotent Consumers

Consumers may receive duplicate messages. Make them idempotent:

```csharp
protected override async Task HandleMessageAsync(...)
{
    var evt = DeserializeMessage<DeviceStatusChangedEvent>(message);

    // Check if already processed
    var exists = await _context.ProcessedEvents
        .AnyAsync(e => e.EventId == evt.SomeUniqueId);

    if (exists) return; // Skip duplicate

    // Process event...
    await _context.ProcessedEvents.AddAsync(new ProcessedEvent
    {
        EventId = evt.SomeUniqueId
    });
}
```

## Troubleshooting

### Connection Issues

Check RabbitMQ is running:

```bash
docker-compose ps rabbitmq
```

Test connection:

```bash
curl http://localhost:15672/api/overview
# Default credentials: guest/guest
```

### Messages Not Being Consumed

1. Check queue bindings in RabbitMQ Management UI (http://localhost:15672)
2. Verify routing keys match between publisher and consumer
3. Check consumer logs for errors

### Memory Issues

Reduce `PrefetchCount` if consumers are slow:

```csharp
protected override ushort PrefetchCount => 1; // Process one at a time
```

## License

MIT - See LICENSE file for details.
