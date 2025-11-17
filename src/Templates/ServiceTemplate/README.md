# ServiceTemplate

A complete microservice template following Clean Architecture principles with gRPC, CQRS, and event-driven messaging.

## Overview

ServiceTemplate is a comprehensive microservice template that demonstrates:

- **Clean Architecture** - Domain, Application, Infrastructure, API layers
- **CQRS Pattern** - Command/Query separation with MediatR
- **gRPC Services** - High-performance RPC communication
- **Event-Driven** - RabbitMQ pub/sub messaging
- **Multiple Hosting Options** - Console, Windows Service, Docker
- **PostgreSQL** - Entity Framework Core with migrations
- **NuGet Packages** - Service and Client libraries

## Project Structure

```
ServiceTemplate/
├── ServiceTemplate.Domain/              # Domain layer (entities, value objects)
│   └── Common/
│       └── BaseEntity.cs               # Base entity with ID and timestamps
├── ServiceTemplate.Application/         # Application layer (CQRS, business logic)
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs   # MediatR validation pipeline
│   │   └── Interfaces/
│   │       └── IApplicationDbContext.cs # Database abstraction
│   └── DependencyInjection.cs          # Application service registration
├── ServiceTemplate.Infrastructure/      # Infrastructure layer (database, messaging)
│   ├── Persistence/
│   │   └── ApplicationDbContext.cs     # EF Core DbContext
│   └── DependencyInjection.cs          # Infrastructure service registration
├── ServiceTemplate.API/                 # gRPC API host (ASP.NET Core)
│   ├── Program.cs                      # API host entry point
│   └── appsettings.json                # Configuration
├── ServiceTemplate.Service/             # Hostable service library (NuGet)
│   ├── ServiceHostBuilder.cs           # Fluent API for service configuration
│   └── ServiceHost.cs                  # Service lifecycle management
├── ServiceTemplate.App/                 # Console application host
│   └── Program.cs                      # Console entry point
├── ServiceTemplate.WindowsService/      # Windows Service host
│   ├── Program.cs                      # Windows Service entry point
│   ├── install-service.ps1             # Installation script
│   └── uninstall-service.ps1           # Uninstallation script
└── ServiceTemplate.Client/              # gRPC client library (NuGet)
    ├── ServiceTemplateClient.cs        # Strongly-typed client
    └── DependencyInjection.cs          # Client registration
```

## Quick Start

### 1. Run as Console Application

```bash
cd ServiceTemplate.App
dotnet run
```

### 2. Run as Docker Container

```bash
docker-compose up service-template
```

### 3. Install as Windows Service

```powershell
# Build the service
dotnet build ServiceTemplate.WindowsService -c Release

# Install (requires Administrator)
cd ServiceTemplate.WindowsService\bin\Release\net8.0
.\install-service.ps1

# Service is now running as "ServiceTemplate"
```

## Development Guide

### Adding a New Entity

1. Create entity in `ServiceTemplate.Domain/Entities/YourEntity.cs`:

```csharp
using ServiceTemplate.Domain.Common;

public class YourEntity : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    private YourEntity() { } // EF Core constructor

    public static YourEntity Create(string name)
    {
        return new YourEntity { Name = name };
    }

    public void UpdateName(string name)
    {
        Name = name;
        MarkAsUpdated();
    }
}
```

2. Add DbSet to `ServiceTemplate.Infrastructure/Persistence/ApplicationDbContext.cs`:

```csharp
public DbSet<YourEntity> YourEntities => Set<YourEntity>();
```

3. Create migration:

```bash
cd ServiceTemplate.Infrastructure
dotnet ef migrations add AddYourEntity
```

### Adding a Command (CQRS)

Create `ServiceTemplate.Application/Features/YourFeature/Commands/YourCommand.cs`:

```csharp
using FluentResults;
using MediatR;
using ServiceTemplate.Application.Common.Interfaces;

public record YourCommand(string Name) : IRequest<Result<string>>;

public class YourCommandHandler : IRequestHandler<YourCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public YourCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(YourCommand request, CancellationToken cancellationToken)
    {
        var entity = YourEntity.Create(request.Name);
        _context.YourEntities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok(entity.Id);
    }
}
```

### Adding a Query (CQRS)

Create `ServiceTemplate.Application/Features/YourFeature/Queries/YourQuery.cs`:

```csharp
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceTemplate.Application.Common.Interfaces;

public record YourQuery(string Id) : IRequest<Result<YourDto>>;

public class YourQueryHandler : IRequestHandler<YourQuery, Result<YourDto>>
{
    private readonly IApplicationDbContext _context;

    public YourQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<YourDto>> Handle(YourQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.YourEntities
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
            return Result.Fail("Entity not found");

        return Result.Ok(new YourDto(entity.Id, entity.Name));
    }
}

public record YourDto(string Id, string Name);
```

### Adding a gRPC Service

1. Define proto in `Workshop.Proto` (shared library)
2. Implement service in `ServiceTemplate.API/Services/YourGrpcService.cs`:

```csharp
using Grpc.Core;
using MediatR;
using Workshop.Common.Extensions;

public class YourGrpcService : YourService.YourServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<YourGrpcService> _logger;

    public YourGrpcService(IMediator mediator, ILogger<YourGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<YourResponse> YourMethod(YourRequest request, ServerCallContext context)
    {
        var command = new YourCommand(request.Name);
        var result = await _mediator.Send(command, context.CancellationToken);

        return await result.MatchAsync(
            id => Task.FromResult(new YourResponse { Id = id }),
            error => throw error.ToRpcException()
        );
    }
}
```

3. Register in `ServiceTemplate.API/Program.cs`:

```csharp
app.MapGrpcService<YourGrpcService>();
```

### Publishing Events

Inject `IRabbitMQPublisher` and publish events:

```csharp
using Workshop.Messaging.Abstractions;
using Workshop.Messaging.Events;

public class YourCommandHandler : IRequestHandler<YourCommand, Result<string>>
{
    private readonly IRabbitMQPublisher _publisher;

    public async Task<Result<string>> Handle(YourCommand request, CancellationToken cancellationToken)
    {
        // ... do work ...

        await _publisher.PublishAsync(new YourEvent
        {
            EntityId = entity.Id,
            Name = entity.Name
        }, cancellationToken);

        return Result.Ok(entity.Id);
    }
}
```

### Consuming Events

Create consumer in `ServiceTemplate.Infrastructure/Messaging/YourEventConsumer.cs`:

```csharp
using Workshop.Messaging.Implementation;
using Workshop.Messaging.Events;

public class YourEventConsumer : RabbitMQConsumerBase
{
    public YourEventConsumer(IServiceProvider serviceProvider, ILogger<YourEventConsumer> logger)
        : base(serviceProvider, logger, "servicetemplate.yourqueue", "other.event.#")
    {
    }

    protected override async Task HandleMessageAsync(string message, CancellationToken cancellationToken)
    {
        var evt = DeserializeMessage<YourEvent>(message);
        if (evt == null) return;

        _logger.LogInformation("Received event: {EventId}", evt.EntityId);

        // Process event...
    }
}
```

Register in `ServiceTemplate.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddHostedService<YourEventConsumer>();
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=servicetemplate;Username=workshop;Password=workshop123"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "workshop",
    "Password": "workshop123",
    "ExchangeName": "workshop.events",
    "ExchangeType": "topic"
  },
  "Kestrel": {
    "Endpoints": {
      "Grpc": {
        "Url": "http://localhost:8080",
        "Protocols": "Http2"
      }
    }
  }
}
```

## Client Usage

### Install NuGet Package

```bash
dotnet add package ServiceTemplate.Client
```

### Register Client

```csharp
services.AddServiceTemplateClient("http://localhost:5001");
```

### Use Client

```csharp
public class YourService
{
    private readonly ServiceTemplateClient _client;

    public YourService(ServiceTemplateClient client)
    {
        _client = client;
    }

    public async Task DoSomethingAsync()
    {
        var response = await _client.YourMethodAsync(new YourRequest
        {
            Name = "Test"
        });
    }
}
```

## Database Migrations

```bash
# Add migration
cd ServiceTemplate.Infrastructure
dotnet ef migrations add YourMigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Testing

```bash
# Run unit tests
dotnet test tests/ServiceTemplate.UnitTests

# Run integration tests
dotnet test tests/ServiceTemplate.IntegrationTests
```

## Deployment

### Docker

```bash
# Build image
docker build -t servicetemplate:latest -f Dockerfile .

# Run container
docker run -p 8080:8080 servicetemplate:latest
```

### Windows Service

```powershell
# Build release
dotnet publish ServiceTemplate.WindowsService -c Release

# Install service (as Administrator)
cd ServiceTemplate.WindowsService\bin\Release\net8.0\publish
.\install-service.ps1

# Uninstall service (as Administrator)
.\uninstall-service.ps1
```

## Architecture Patterns

### Clean Architecture Layers

1. **Domain** - Business entities, no dependencies
2. **Application** - Business logic, depends on Domain
3. **Infrastructure** - Database, messaging, depends on Application
4. **API** - gRPC endpoints, depends on Infrastructure

### Dependency Flow

```
API → Infrastructure → Application → Domain
```

All dependencies point inward. Domain has zero dependencies.

### CQRS Pattern

- **Commands** - Mutate state, return Result<T>
- **Queries** - Read state, return Result<T>
- Handled by MediatR pipeline with validation

### Event-Driven

- Publish domain events to RabbitMQ
- Other services subscribe via topic exchange
- Loose coupling between services

## Best Practices

1. **Always use FluentResults** - No exceptions for business logic
2. **Validate inputs** - Use FluentValidation in commands/queries
3. **Log extensively** - Use ILogger for observability
4. **Use DTOs** - Don't expose entities in gRPC
5. **Test thoroughly** - Unit tests for logic, integration tests for infrastructure
6. **Database per service** - Own your data
7. **Event versioning** - Never break event contracts
8. **Idempotent consumers** - Handle duplicate events gracefully

## Troubleshooting

### Database Connection Failed

Check PostgreSQL is running and connection string is correct:

```bash
docker-compose ps postgres-servicetemplate
```

### RabbitMQ Connection Failed

Check RabbitMQ is running:

```bash
docker-compose ps rabbitmq
# Visit http://localhost:15672 (workshop/workshop123)
```

### gRPC Service Not Found

Enable gRPC reflection in development and use grpcurl:

```bash
grpcurl -plaintext localhost:8080 list
```

## License

MIT License - see LICENSE file for details

## Support

For issues or questions, please open an issue on GitHub.

---

**Built with Clean Architecture for Skidata Workshop 2025**
