# 🏗️ Clean Architecture Workshop - Skidata 2025

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Welcome to the **Skidata Clean Architecture Workshop 2025**! This repository contains a complete, production-ready implementation of a distributed microservices system built with:

- ✅ **Clean Architecture** with vertical slice CQRS
- ✅ **gRPC** for synchronous service-to-service communication
- ✅ **RabbitMQ** for asynchronous event-driven messaging
- ✅ **PostgreSQL** for persistent storage
- ✅ **Docker Compose** for local orchestration
- ✅ **GitHub Packages** for NuGet package management
- ✅ **FluentResults** for Railway-Oriented Programming (no exceptions)

---

## 📚 Table of Contents

- [Workshop Overview](#-workshop-overview)
- [Architecture](#-architecture)
- [Repository Structure](#-repository-structure)
- [Getting Started](#-getting-started)
- [Services](#-services)
- [Shared Libraries](#-shared-libraries)
- [Running the Workshop](#-running-the-workshop)
- [Documentation](#-documentation)
- [Contributing](#-contributing)

---

## 🎯 Workshop Overview

This workshop teaches senior developers how to build a **real-world distributed system** that handles device management, monitoring, and diagnostics - similar to Skidata's production systems for gates, lifts, and access control devices.

### Learning Objectives

By the end of this workshop, participants will:

1. **Understand Clean Architecture** - Domain, Application, Infrastructure, and API layers
2. **Master CQRS with MediatR** - Commands and Queries with pipeline behaviors
3. **Build gRPC services** - Contract-first design with Protocol Buffers
4. **Publish NuGet packages** - Package creation, versioning, and GitHub Packages
5. **Implement event-driven patterns** - RabbitMQ pub/sub with RabbitMQ
6. **Orchestrate with Docker** - Multi-container applications with Docker Compose
7. **Apply Result pattern** - Railway-Oriented Programming instead of exception handling

### Workshop Duration

**1 Full Day** (09:00 - 17:00)

- Morning: Clean Architecture, CQRS, gRPC fundamentals
- Afternoon: RabbitMQ integration, Docker orchestration, final project

---

## 🏛️ Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      Workshop Orchestrator                       │
│              (Coordinates all services via gRPC)                 │
└────────────┬─────────────────┬─────────────────┬────────────────┘
             │                 │                 │
    ┌────────▼────────┐ ┌─────▼──────┐ ┌───────▼────────┐
    │ Device Service  │ │ Monitoring │ │  Diagnostics   │
    │   (Port 5001)   │ │  Service   │ │    Service     │
    │                 │ │ (Port 5002)│ │  (Port 5003)   │
    └────────┬────────┘ └─────┬──────┘ └───────┬────────┘
             │                │                 │
             │    ┌───────────▼─────────────┐   │
             └────►      RabbitMQ Bus       ◄───┘
                  │  (Event Distribution)   │
                  └─────────────────────────┘
                             │
             ┌───────────────┼───────────────┐
             │               │               │
      ┌──────▼──────┐ ┌─────▼──────┐ ┌─────▼──────┐
      │ PostgreSQL  │ │ PostgreSQL │ │ PostgreSQL │
      │ (DeviceDB)  │ │(MonitorDB) │ │(DiagnostDB)│
      └─────────────┘ └────────────┘ └────────────┘
```

### Communication Patterns

1. **Synchronous (gRPC)**
   - Get device status
   - Query alerts
   - Fetch diagnostics

2. **Asynchronous (RabbitMQ)**
   - Device status changed events
   - Alert triggered notifications
   - Health check failures

---

## 📁 Repository Structure

```
clean-architecture/
│
├── src/
│   ├── Shared/                        # Shared libraries (published as NuGet)
│   │   ├── Workshop.Common/           # Common utilities, Result pattern
│   │   ├── Workshop.Proto/            # Protocol Buffer definitions
│   │   ├── Workshop.Contracts/        # Generated C# contracts
│   │   └── Workshop.Messaging/        # RabbitMQ abstractions
│   │
│   ├── Templates/                     # Starting templates for teams
│   │   ├── ServiceTemplate/           # Clean Architecture template
│   │   └── ClientTemplate/            # gRPC client template
│   │
│   ├── Services/                      # Microservices implementations
│   │   ├── DeviceService/             # Device management service
│   │   ├── MonitoringService/         # Real-time monitoring
│   │   └── DiagnosticsService/        # Health checks and logging
│   │
│   ├── Orchestrator/                  # Final integration project
│   │   └── Workshop.Orchestrator/
│   │
│   └── UI/                            # Future: React dashboard
│
├── tests/                             # Unit and integration tests
│   ├── DeviceService.UnitTests/
│   ├── DeviceService.IntegrationTests/
│   └── ...
│
├── docs/                              # Workshop documentation
│   ├── workshop-overview.md
│   ├── setup-instructions.md
│   ├── clean-architecture-guide.md
│   ├── grpc-guide.md
│   ├── rabbitmq-guide.md
│   └── ...
│
├── scripts/                           # Helper scripts
│   ├── setup-dev-environment.sh
│   ├── publish-all-shared.sh
│   ├── build-all.sh
│   └── ...
│
├── .github/workflows/                 # CI/CD pipelines
├── docker-compose.yml                 # Docker orchestration
├── NuGet.Config                       # Package sources
└── Directory.Packages.props           # Central package management
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/)

### Quick Start

1. **Clone the repository**

```bash
git clone https://github.com/klika-skidata-workshop-2025/clean-architecture.git
cd clean-architecture
```

2. **Pack shared libraries to local feed**

The workshop uses NuGet packages for shared libraries. First, pack them to the local feed:

```bash
# Pack all three shared libraries
dotnet pack src/Shared/Workshop.Common/Workshop.Common.csproj --configuration Release --output ./local-packages
dotnet pack src/Shared/Workshop.Proto/Workshop.Proto.csproj --configuration Release --output ./local-packages
dotnet pack src/Shared/Workshop.Messaging/Workshop.Messaging.csproj --configuration Release --output ./local-packages
```

> **Note:** See [NUGET-SETUP.md](NUGET-SETUP.md) for detailed NuGet workflow documentation.

3. **Restore packages**

```bash
dotnet restore
```

> **Optional:** For production use with GitHub Packages, you'll need to set up authentication. See [NUGET-SETUP.md](NUGET-SETUP.md#production-deployment) for details.

4. **Start infrastructure (PostgreSQL + RabbitMQ)**

```bash
docker-compose up -d postgres-device postgres-monitoring postgres-diagnostics rabbitmq
```

5. **Run a service**

```bash
cd src/Services/DeviceService/DeviceService.App
dotnet run
```

6. **Run the entire system**

```bash
docker-compose up
```

---

## 🔧 Services

### Device Service (Port 5001)

**Purpose**: Entry point for device events (gates, lifts, counters)

**gRPC Endpoints**:
- `GetDeviceStatus(deviceId)` - Query current device status
- `UpdateDevice(deviceId, status)` - Update device configuration
- `SimulateDeviceEvent(deviceId, eventType)` - Trigger a simulated event

**Events Published**:
- `DeviceStatusChanged` - When device status changes
- `DeviceHeartbeat` - Periodic health signal

### Monitoring Service (Port 5002)

**Purpose**: Rule-based monitoring and alerting

**gRPC Endpoints**:
- `GetActiveAlerts()` - Query current alerts
- `AcknowledgeAlert(alertId)` - Dismiss an alert

**Events Consumed**:
- `DeviceStatusChanged` (from Device Service)

**Events Published**:
- `AlertTriggered` - When monitoring rule matches

**Integration**: Calls Device Service via gRPC when alert requires action

### Diagnostics Service (Port 5003)

**Purpose**: Centralized logging and health monitoring

**gRPC Endpoints**:
- `GetSystemHealth()` - Aggregated health status
- `GetErrorLogs(timeRange)` - Query error logs

**Events Consumed**:
- ALL events (wildcard subscription)

**Background Jobs**: Periodic health checks (every 30 seconds)

---

## 📦 Shared Libraries

All shared libraries are published as NuGet packages to GitHub Packages.

### Workshop.Common

Contains:
- `Result<T>` - FluentResults integration
- `Error` helpers
- Extension methods
- Common constants

### Workshop.Proto

Protocol Buffer definitions for all services:
- `device.proto` - Device service contracts
- `monitoring.proto` - Monitoring service contracts
- `diagnostics.proto` - Diagnostics service contracts
- `common.proto` - Shared types

### Workshop.Contracts

Generated C# code from `.proto` files (auto-generated).

### Workshop.Messaging

RabbitMQ abstractions:
- `IRabbitMQPublisher` - Publish events
- `RabbitMQConsumerBase` - Base class for consumers
- Event DTOs (DeviceStatusChangedEvent, etc.)

---

## 🏃 Running the Workshop

### Option 1: Console Applications (Development)

Each service has a console app that can be run independently:

```bash
# Terminal 1 - Device Service
cd src/Services/DeviceService/DeviceService.App
dotnet run

# Terminal 2 - Monitoring Service
cd src/Services/MonitoringService/MonitoringService.App
dotnet run

# Terminal 3 - Diagnostics Service
cd src/Services/DiagnosticsService/DiagnosticsService.App
dotnet run

# Terminal 4 - Orchestrator
cd src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.API
dotnet run
```

**Interactive Controls**:
- Press **ENTER** to stop the service gracefully
- Press **Ctrl+C** for immediate shutdown

### Option 2: Docker Compose (Production-like)

```bash
# Start all services
docker-compose up

# Start in detached mode
docker-compose up -d

# View logs
docker-compose logs -f device-service

# Stop all services
docker-compose down

# Clean up volumes
docker-compose down -v
```

### Option 3: Windows Services (On-Premises)

Each service has a Windows Service host:

```powershell
# Publish the service
dotnet publish src/Services/DeviceService/DeviceService.WindowsService -c Release -o C:\Services\DeviceService

# Install as Windows Service
sc.exe create "SkidataDeviceService" binPath="C:\Services\DeviceService\DeviceService.WindowsService.exe"

# Start
sc.exe start "SkidataDeviceService"

# Stop
sc.exe stop "SkidataDeviceService"

# Uninstall
sc.exe delete "SkidataDeviceService"
```

---

## 📖 Documentation

Comprehensive guides are available in the [`docs/`](docs/) folder:

- [Workshop Overview](docs/workshop-overview.md) - High-level plan and learning objectives
- [Setup Instructions](docs/setup-instructions.md) - Detailed environment setup
- [Clean Architecture Guide](docs/clean-architecture-guide.md) - Architecture patterns explained
- [gRPC Guide](docs/grpc-guide.md) - Protocol Buffers and gRPC usage
- [RabbitMQ Guide](docs/rabbitmq-guide.md) - Event-driven messaging patterns
- [Docker Guide](docs/docker-guide.md) - Container orchestration
- [NuGet Setup](NUGET-SETUP.md) - **NEW!** NuGet package workflow and CI/CD setup
- [Team Tasks](docs/team-tasks.md) - What each team will build
- [Troubleshooting](docs/troubleshooting.md) - Common issues and solutions

---

## 🧪 Testing

### Run Unit Tests

```bash
dotnet test tests/DeviceService.UnitTests
```

### Run Integration Tests

```bash
# Requires Docker for Testcontainers (PostgreSQL + RabbitMQ)
dotnet test tests/DeviceService.IntegrationTests
```

### Run All Tests

```bash
dotnet test
```

---

## 🛠️ Development

### Build All Projects

```bash
dotnet build
```

### Publish Shared Packages

```bash
./scripts/publish-all-shared.sh
```

### Generate Proto Code

```bash
./scripts/generate-all-protos.sh
```

---

## 🤝 Contributing

This is a workshop repository. Contributions are welcome!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Authors

**Klika Skidata Workshop Team**

- Workshop Lead: Benjamin Sehic
- Organization: [Klika Skidata Workshop 2025](https://github.com/klika-skidata-workshop-2025)

---

## 🙏 Acknowledgments

- Inspired by real-world Skidata production systems
- Built with industry-standard tools and patterns
- Designed for senior developers to learn production-grade architecture

---

## 📞 Support

For questions or issues:
- Open an [Issue](https://github.com/klika-skidata-workshop-2025/clean-architecture/issues)
- Contact the workshop organizers

---

**Happy Coding! 🚀**
