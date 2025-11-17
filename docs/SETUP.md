# Workshop Setup Guide

Complete setup instructions for the Skidata Clean Architecture Workshop 2025.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Initial Setup](#initial-setup)
- [GitHub Packages Authentication](#github-packages-authentication)
- [Building the Project](#building-the-project)
- [Running with Docker](#running-with-docker)
- [Running Locally](#running-locally)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

1. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (8.0.100 or later)
   ```bash
   dotnet --version
   # Should output: 8.0.x
   ```

2. **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (latest version)
   ```bash
   docker --version
   docker-compose --version
   ```

3. **[Git](https://git-scm.com/downloads)** (latest version)
   ```bash
   git --version
   ```

### Recommended IDE (choose one)

- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (v17.8+)
  - Workload: "ASP.NET and web development"
  - Workload: ".NET desktop development"

- **[JetBrains Rider](https://www.jetbrains.com/rider/)** (2023.3+)

- **[Visual Studio Code](https://code.visualstudio.com/)** with extensions:
  - C# Dev Kit
  - Docker
  - REST Client (for testing gRPC)

### Optional Tools

- **[grpcurl](https://github.com/fullstorydev/grpcurl)** - Test gRPC services from command line
- **[Postman](https://www.postman.com/)** - REST/gRPC client
- **[pgAdmin](https://www.pgadmin.org/)** - PostgreSQL management UI

---

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/klika-skidata-workshop-2025/clean-architecture.git
cd clean-architecture
```

### 2. Verify Directory Structure

```bash
ls -la
# Should see: src/, tests/, docs/, scripts/, docker-compose.yml, etc.
```

---

## GitHub Packages Authentication

The workshop uses GitHub Packages as a NuGet feed for shared libraries.

### Step 1: Generate GitHub Personal Access Token (PAT)

1. Go to [GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)](https://github.com/settings/tokens)
2. Click "Generate new token (classic)"
3. Give it a name: "Skidata Workshop NuGet"
4. Select scopes:
   - ✅ `read:packages` - Download packages
   - ✅ `write:packages` - Publish packages (if you're building services)
5. Click "Generate token"
6. **Copy the token** (you won't see it again!)

### Step 2: Configure Environment Variable

**Windows PowerShell:**
```powershell
$env:GITHUB_TOKEN="ghp_YOUR_TOKEN_HERE"

# Make it permanent (current user):
[Environment]::SetEnvironmentVariable("GITHUB_TOKEN", "ghp_YOUR_TOKEN_HERE", "User")
```

**Linux/macOS:**
```bash
export GITHUB_TOKEN="ghp_YOUR_TOKEN_HERE"

# Make it permanent (add to ~/.bashrc or ~/.zshrc):
echo 'export GITHUB_TOKEN="ghp_YOUR_TOKEN_HERE"' >> ~/.bashrc
source ~/.bashrc
```

### Step 3: Verify NuGet Authentication

```bash
dotnet nuget list source
# Should show "github" source
```

Test authentication:
```bash
dotnet restore
# Should succeed without authentication errors
```

---

## Building the Project

### Option 1: Build Script (Recommended)

**Windows:**
```powershell
.\scripts\build-all.ps1
```

**Linux/macOS:**
```bash
chmod +x scripts/build-all.sh
./scripts/build-all.sh
```

### Option 2: Manual Build

```bash
# Restore packages
dotnet restore

# Build shared libraries
dotnet build src/Shared/Workshop.Common/Workshop.Common.csproj -c Release
dotnet build src/Shared/Workshop.Proto/Workshop.Proto.csproj -c Release
dotnet build src/Shared/Workshop.Messaging/Workshop.Messaging.csproj -c Release

# Build services (when available)
# dotnet build src/Services/DeviceService/DeviceService.sln -c Release
# dotnet build src/Services/MonitoringService/MonitoringService.sln -c Release
# dotnet build src/Services/DiagnosticsService/DiagnosticsService.sln -c Release

# Build orchestrator (when available)
# dotnet build src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.sln -c Release
```

---

## Running with Docker

### Step 1: Start Infrastructure Only

Start PostgreSQL and RabbitMQ:

```bash
docker-compose up -d postgres-device postgres-monitoring postgres-diagnostics rabbitmq
```

Verify services are healthy:

```bash
docker-compose ps
# All services should show (healthy)
```

### Step 2: Access Management UIs

- **RabbitMQ Management**: http://localhost:15672
  - Username: `workshop`
  - Password: `workshop123`

- **PostgreSQL**:
  - Device DB: `localhost:5432`
  - Monitoring DB: `localhost:5433`
  - Diagnostics DB: `localhost:5434`
  - Username: `workshop`
  - Password: `workshop123`

### Step 3: Start All Services

```bash
# Start everything
docker-compose up

# Or start in detached mode
docker-compose up -d

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f device-service
```

### Step 4: Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

---

## Running Locally (Without Docker)

### Step 1: Start Infrastructure

```bash
docker-compose up -d postgres-device postgres-monitoring postgres-diagnostics rabbitmq
```

### Step 2: Run Services Individually

**Terminal 1 - Device Service:**
```bash
cd src/Services/DeviceService/DeviceService.App
dotnet run
# Service starts on http://localhost:5001
```

**Terminal 2 - Monitoring Service:**
```bash
cd src/Services/MonitoringService/MonitoringService.App
dotnet run
# Service starts on http://localhost:5002
```

**Terminal 3 - Diagnostics Service:**
```bash
cd src/Services/DiagnosticsService/DiagnosticsService.App
dotnet run
# Service starts on http://localhost:5003
```

**Terminal 4 - Orchestrator:**
```bash
cd src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.API
dotnet run
# Service starts on http://localhost:5000
```

### Interactive Console Controls

When running services locally:
- **Press ENTER** - Graceful shutdown
- **Press Ctrl+C** - Immediate shutdown

---

## Verification

### Check Service Health

**Device Service:**
```bash
# Using curl
curl http://localhost:5001/health

# Using grpcurl (if installed)
grpcurl -plaintext localhost:5001 list
```

**Monitoring Service:**
```bash
curl http://localhost:5002/health
```

**Diagnostics Service:**
```bash
curl http://localhost:5003/health
```

### Check RabbitMQ

1. Open http://localhost:15672
2. Go to "Exchanges" tab
3. Verify "workshop.events" exchange exists
4. Go to "Queues" tab
5. Verify service queues exist

### Check PostgreSQL

**Using psql:**
```bash
# Connect to Device DB
psql -h localhost -p 5432 -U workshop -d devicedb

# List tables
\dt

# Exit
\q
```

**Using Docker:**
```bash
docker exec -it workshop-postgres-device psql -U workshop -d devicedb
```

---

## Troubleshooting

### Issue: NuGet Authentication Failed

**Error:**
```
error NU1301: Unable to load the service index for source https://nuget.pkg.github.com/...
```

**Solution:**
1. Verify GITHUB_TOKEN environment variable is set
2. Verify token has `read:packages` scope
3. Try clearing NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

### Issue: Docker Port Conflicts

**Error:**
```
Bind for 0.0.0.0:5432 failed: port is already allocated
```

**Solution:**
1. Check what's using the port:
   ```bash
   # Windows
   netstat -ano | findstr :5432

   # Linux/macOS
   lsof -i :5432
   ```

2. Stop the conflicting service or change port in docker-compose.yml

### Issue: RabbitMQ Connection Failed

**Error:**
```
RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
```

**Solution:**
1. Verify RabbitMQ is running:
   ```bash
   docker-compose ps rabbitmq
   ```

2. Check RabbitMQ logs:
   ```bash
   docker-compose logs rabbitmq
   ```

3. Restart RabbitMQ:
   ```bash
   docker-compose restart rabbitmq
   ```

### Issue: Database Migration Failed

**Error:**
```
Npgsql.PostgresException: 42P01: relation "Devices" does not exist
```

**Solution:**
1. Database migrations haven't run. Check service logs for migration errors
2. Manually run migrations:
   ```bash
   cd src/Services/DeviceService/DeviceService.Infrastructure
   dotnet ef database update
   ```

### Issue: gRPC Service Not Responding

**Error:**
```
Grpc.Core.RpcException: Status(StatusCode="Unavailable", Detail="Error connecting to subchannel.")
```

**Solution:**
1. Verify service is running:
   ```bash
   docker-compose ps device-service
   ```

2. Check service logs:
   ```bash
   docker-compose logs device-service
   ```

3. Verify port mapping in docker-compose.yml

### Issue: Build Fails with CS0246 (Type Not Found)

**Error:**
```
error CS0246: The type or namespace name 'Workshop' could not be found
```

**Solution:**
1. Restore packages:
   ```bash
   dotnet restore
   ```

2. Build shared libraries first:
   ```bash
   dotnet build src/Shared/Workshop.Common/Workshop.Common.csproj
   dotnet build src/Shared/Workshop.Proto/Workshop.Proto.csproj
   dotnet build src/Shared/Workshop.Messaging/Workshop.Messaging.csproj
   ```

3. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

---

## Next Steps

After successful setup:

1. **Read the Architecture Guide**: [docs/clean-architecture-guide.md](clean-architecture-guide.md)
2. **Explore gRPC Contracts**: [docs/grpc-guide.md](grpc-guide.md)
3. **Understand Event Flow**: [docs/rabbitmq-guide.md](rabbitmq-guide.md)
4. **Start Building**: Follow the workshop tasks in [docs/team-tasks.md](team-tasks.md)

---

## Support

For issues or questions:
- Check [docs/troubleshooting.md](troubleshooting.md)
- Open an issue on GitHub
- Contact workshop organizers

---

**Happy Coding! 🚀**
