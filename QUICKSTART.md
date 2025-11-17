# 🚀 Clean Architecture Workshop - Quick Start Guide

**Welcome to the Skidata Clean Architecture Workshop 2025!**

This guide will get you up and running in **under 10 minutes**.

---

## ✅ Prerequisites Check

Before starting, ensure you have:

- [x] **.NET 8 SDK** (8.0.100 or later) - `dotnet --version`
- [x] **Docker Desktop** (running) - `docker --version`
- [x] **Git** - `git --version`
- [x] **GitHub Personal Access Token** with `read:packages` scope (optional)

---

## 🚀 Quick Start (3 Steps)

### Step 1: Clone and Build (5 minutes)

```powershell
# Navigate to project
cd "path\to\skidata-workshop-2025\clean-architecture"

# Build all shared libraries
dotnet build src/Shared/Workshop.Common/Workshop.Common.csproj -c Release
dotnet build src/Shared/Workshop.Proto/Workshop.Proto.csproj -c Release
dotnet build src/Shared/Workshop.Messaging/Workshop.Messaging.csproj -c Release

# Or use the build script
.\scripts\build-all.ps1
```

---

### Step 2: Start Infrastructure (3 minutes)

```powershell
# Start all infrastructure services
docker-compose up -d

# Wait for services to be healthy (30 seconds)
Start-Sleep -Seconds 30

# Check status
docker-compose ps
```

**Access Points**:
- RabbitMQ Management: http://localhost:15672 (workshop / workshop123)
- Device Service: http://localhost:5001
- Monitoring Service: http://localhost:5002
- Diagnostics Service: http://localhost:5003

---

### Step 3: Test the System (2 minutes)

```powershell
# Check all services are healthy
docker-compose ps

# View logs
docker-compose logs -f device-service
docker-compose logs -f monitoring-service
docker-compose logs -f diagnostics-service

# Or test with grpcurl (if installed)
grpcurl -plaintext localhost:5001 list
grpcurl -plaintext localhost:5001 grpc.health.v1.Health/Check
```

---

## 🎯 What You'll Learn

This workshop demonstrates a **production-ready microservices architecture** with:

### Architecture Patterns
- **Clean Architecture** - Domain, Application, Infrastructure, API layers
- **CQRS** - Command/Query separation with MediatR
- **Railway-Oriented Programming** - FluentResults for error handling (no exceptions!)
- **Event-Driven Architecture** - RabbitMQ topic exchange with wildcards
- **Database Per Service** - PostgreSQL with EF Core per microservice
- **Service as Library** - Unique StartAsync/StopAsync pattern for flexible hosting

### Technologies
- **.NET 8** - Latest C# features
- **gRPC** - High-performance RPC with Protocol Buffers
- **RabbitMQ** - Event streaming and message routing
- **PostgreSQL** - Relational database with EF Core
- **Docker Compose** - Full stack orchestration
- **FluentValidation** - Input validation
- **MediatR** - In-process messaging

### Three Microservices
1. **DeviceService** - Manages gates, lifts, counters, controls
2. **MonitoringService** - Alert management with rule engine (consumes device events!)
3. **DiagnosticsService** - Health monitoring and error logging

---

## 🎓 Workshop Activities

### Activity 1: Explore the Architecture (30 minutes)

**Goal**: Understand Clean Architecture layers

1. Open [DeviceService.Domain/Entities/Device.cs](src/Services/DeviceService/DeviceService.Domain/Entities/Device.cs)
   - Review the Device entity - no dependencies on infrastructure!
   - See how `RecordHeartbeat()` and `IsOnline()` contain pure business logic

2. Open [DeviceService.Application/Features/Devices/Commands/RegisterDevice/RegisterDeviceCommand.cs](src/Services/DeviceService/DeviceService.Application/Features/Devices/Commands/RegisterDevice/RegisterDeviceCommand.cs)
   - See CQRS command pattern with MediatR
   - Notice FluentValidation for input validation
   - See how it publishes events to RabbitMQ

3. Open [DeviceService.Infrastructure/Data/ApplicationDbContext.cs](src/Services/DeviceService/DeviceService.Infrastructure/Data/ApplicationDbContext.cs)
   - See EF Core configuration
   - Notice entity configuration is separated from domain

4. Open [DeviceService.API/Services/DeviceGrpcService.cs](src/Services/DeviceService/DeviceService.API/Services/DeviceGrpcService.cs)
   - See how gRPC maps to CQRS commands/queries
   - Notice error handling with FluentResults

**Discussion**: Why is this better than a traditional layered architecture?

---

### Activity 2: Event-Driven Architecture (45 minutes)

**Goal**: See events flowing between services

1. **Start all services**:
```powershell
docker-compose up -d
docker-compose logs -f device-service monitoring-service
```

2. **Open RabbitMQ Management**: http://localhost:15672
   - Login: workshop / workshop123
   - Go to "Exchanges" tab - find `workshop.events`
   - Go to "Queues" tab - see consumer queues

3. **Trigger a device event**:
```powershell
# Use grpcurl to simulate a device event
grpcurl -plaintext -d '{
  "device_id": "GATE-001",
  "status": "OFFLINE"
}' localhost:5001 workshop.device.DeviceService/SimulateDeviceEvent
```

4. **Watch what happens**:
   - DeviceService publishes `device.status.changed` event to RabbitMQ
   - MonitoringService consumes the event via [DeviceEventConsumer.cs](src/Services/MonitoringService/MonitoringService.Infrastructure/Messaging/DeviceEventConsumer.cs)
   - MonitoringService evaluates monitoring rules
   - If rule matches, creates an Alert
   - MonitoringService publishes `monitoring.alert.triggered` event

5. **Check the results**:
```powershell
# Query alerts in MonitoringService
grpcurl -plaintext localhost:5002 workshop.monitoring.MonitoringService/GetActiveAlerts
```

**Discussion**: What are the benefits of event-driven architecture vs direct service calls?

---

### Activity 3: Add a New Feature (60 minutes)

**Goal**: Practice CQRS by adding a new command

**Task**: Add a `DeactivateDevice` command to DeviceService

1. **Create the command**:
   - Copy `RegisterDeviceCommand.cs` as a template
   - Create `DeactivateDeviceCommand.cs` in `DeviceService.Application/Features/Devices/Commands/DeactivateDevice/`
   - Command should:
     - Take `DeviceId` as input
     - Find the device
     - Set status to `Inactive`
     - Publish `DeviceStatusChangedEvent`

2. **Add gRPC method**:
   - Add RPC to [device.proto](src/Shared/Workshop.Proto/Protos/device.proto)
   - Rebuild Workshop.Proto to regenerate C# code
   - Implement in [DeviceGrpcService.cs](src/Services/DeviceService/DeviceService.API/Services/DeviceGrpcService.cs)

3. **Test it**:
```powershell
# Rebuild and restart
docker-compose up -d --build device-service

# Call the new method
grpcurl -plaintext -d '{"device_id": "GATE-001"}' localhost:5001 workshop.device.DeviceService/DeactivateDevice
```

**Discussion**: How easy was it to add a new feature? What changed and what stayed the same?

---

### Activity 4: Understand the Service Library Pattern (30 minutes)

**Goal**: See how services can be hosted multiple ways

1. **Review the pattern**:
   - Open [DeviceService.Service/ServiceHostBuilder.cs](src/Services/DeviceService/DeviceService.Service/ServiceHostBuilder.cs)
   - Open [DeviceService.Service/ServiceHost.cs](src/Services/DeviceService/DeviceService.Service/ServiceHost.cs)
   - See how `StartAsync()` and `StopAsync()` provide lifecycle management

2. **See the different hosts**:
   - Console App: [DeviceService.ConsoleApp/Program.cs](src/Services/DeviceService/DeviceService.ConsoleApp/Program.cs)
   - Windows Service: [DeviceService.WindowsService/Program.cs](src/Services/DeviceService/DeviceService.WindowsService/Program.cs)
   - Docker: [DeviceService/Dockerfile](src/Services/DeviceService/Dockerfile)

3. **Run as console app**:
```powershell
cd src/Services/DeviceService/DeviceService.ConsoleApp
dotnet run
```

**Discussion**: What are the benefits of this pattern? When would you use each hosting option?

---

## 🐛 Common Issues & Quick Fixes

### Issue: Docker Port Already in Use
```powershell
# Check what's using the port
netstat -ano | findstr :5432

# Change port in docker-compose.yml or stop the conflicting process
```

### Issue: Services Not Starting
```powershell
# Check logs
docker-compose logs device-service

# Common fixes:
# 1. Database not ready - wait 30 seconds
# 2. Port conflict - change port in docker-compose.yml
# 3. Build error - rebuild with: docker-compose up -d --build
```

### Issue: RabbitMQ Not Showing Messages
```powershell
# Check if exchange exists
# Go to http://localhost:15672 -> Exchanges tab
# The "workshop.events" exchange is created on first publish

# Check consumer is connected
# Go to Queues tab - should see monitoring service queue
```

---

## 💡 Key Concepts to Understand

### 1. Clean Architecture Layers
```
Domain Layer (Entities, Value Objects)
    ↓
Application Layer (Commands, Queries, Validators)
    ↓
Infrastructure Layer (DbContext, RabbitMQ, External Services)
    ↓
API Layer (gRPC Services, Controllers)
```

**Dependency Rule**: Inner layers never depend on outer layers!

### 2. CQRS Pattern
- **Commands**: Change state (RegisterDevice, UpdateDevice)
- **Queries**: Return data (GetDeviceStatus, ListDevices)
- Separation allows different optimization strategies

### 3. FluentResults (Railway-Oriented Programming)
```csharp
// Instead of exceptions:
if (device == null)
    throw new NotFoundException();

// We use:
if (device == null)
    return Result.Fail("Device not found").WithError("NOT_FOUND");
```

### 4. Event-Driven Communication
```
DeviceService → RabbitMQ (device.status.changed)
                    ↓
               MonitoringService → Evaluate Rules → Create Alert
                    ↓
               RabbitMQ (monitoring.alert.triggered)
```

### 5. Database Per Service
- Each service has its own database
- No shared tables between services
- Services communicate via events or gRPC

---

## 📚 Further Reading

**Key Files to Study**:
1. [PROGRESS.md](PROGRESS.md) - Complete project status
2. [Workshop.Common README](src/Shared/Workshop.Common/README.md) - Error handling patterns
3. [Workshop.Messaging README](src/Shared/Workshop.Messaging/README.md) - Event patterns
4. [DeviceEventConsumer.cs](src/Services/MonitoringService/MonitoringService.Infrastructure/Messaging/DeviceEventConsumer.cs) - Event consumer example

**External Resources**:
- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQRS Pattern: https://martinfowler.com/bliki/CQRS.html
- Railway-Oriented Programming: https://fsharpforfunandprofit.com/rop/
- gRPC in .NET: https://learn.microsoft.com/en-us/aspnet/core/grpc/

---

## 🚀 Next Steps

After completing the workshop, consider:

1. **Add Orchestrator** - REST API that calls all services
2. **Add API Gateway** - Single entry point with routing
3. **Add Distributed Tracing** - OpenTelemetry integration
4. **Add Resilience** - Polly for retry/circuit breaker
5. **Add Authentication** - JWT tokens with IdentityServer
6. **Add Caching** - Redis for read-heavy queries
7. **Add Testing** - Unit tests, integration tests, contract tests

---

## 📞 Support

**Need Help?**
- Check [PROGRESS.md](PROGRESS.md) for project status
- Review service READMEs in each service folder
- Check Docker logs: `docker-compose logs -f [service-name]`
- Review proto definitions in [Workshop.Proto/Protos/](src/Shared/Workshop.Proto/Protos/)

**Common Commands**:
```powershell
# Restart everything
docker-compose down && docker-compose up -d

# Rebuild specific service
docker-compose up -d --build device-service

# View all logs
docker-compose logs -f

# Check service health
grpcurl -plaintext localhost:5001 grpc.health.v1.Health/Check
```

---

**Happy learning! This architecture scales from small projects to enterprise systems. 🎯**
