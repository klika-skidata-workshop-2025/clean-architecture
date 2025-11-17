# Workshop Orchestrator

**REST API that demonstrates orchestration of Device, Monitoring, and Diagnostics microservices**

---

## Overview

The Workshop Orchestrator is a REST API that provides a unified interface to interact with all three microservices in the workshop. It demonstrates:

- **Service Orchestration** - Coordinating multiple gRPC services
- **REST to gRPC Translation** - Converting REST requests to gRPC calls
- **Complex Workflows** - Multi-step business processes across services
- **Swagger Documentation** - Interactive API documentation

---

## Features

### 1. Device Management
- Register new devices
- Get device status
- List all devices
- Update device status
- Simulate device events

### 2. Monitoring Operations
- Get active alerts
- View alert history
- Acknowledge alerts
- Create monitoring rules
- List monitoring rules

### 3. Diagnostics
- Get system health across all services
- View error logs

### 4. Workflow Demonstrations
- **Complete Workflow** - Full end-to-end demonstration
- **Device Health Report** - Aggregate health across all devices

---

## Architecture

```
REST Client (Swagger UI / Postman / curl)
        ↓
Workshop.Orchestrator (ASP.NET Core REST API)
        ↓
   gRPC Clients
        ↓
┌────────────────┬────────────────────┬────────────────────┐
│ Device Service │ Monitoring Service │ Diagnostics Service│
└────────────────┴────────────────────┴────────────────────┘
```

### Key Components

1. **Controllers** - REST API endpoints
   - `DevicesController` - Device management
   - `MonitoringController` - Alert and rule management
   - `DiagnosticsController` - System health
   - `WorkflowsController` - Complex workflows

2. **Orchestrators** - Business logic and service coordination
   - `DeviceOrchestrator` - Device operations
   - `MonitoringOrchestrator` - Monitoring operations
   - `DiagnosticsOrchestrator` - Diagnostics operations
   - `WorkflowOrchestrator` - Complex workflows

3. **gRPC Clients** - Auto-injected by DI
   - `DeviceService.DeviceServiceClient`
   - `MonitoringService.MonitoringServiceClient`
   - `DiagnosticsService.DiagnosticsServiceClient`

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- All three microservices running (Device, Monitoring, Diagnostics)
- Docker (optional)

### Running Locally

```bash
cd src/Orchestrator/Workshop.Orchestrator
dotnet run
```

Access Swagger UI: http://localhost:5000

### Running with Docker

```bash
# From repository root
docker-compose up -d orchestrator
```

Access Swagger UI: http://localhost:5000

---

## API Endpoints

### Devices

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/devices` | Register new device |
| GET | `/api/devices/{deviceId}` | Get device status |
| GET | `/api/devices` | List all devices |
| PUT | `/api/devices/{deviceId}` | Update device |
| POST | `/api/devices/{deviceId}/simulate` | Simulate device event |

### Monitoring

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/monitoring/alerts/active` | Get active alerts |
| GET | `/api/monitoring/alerts/history/{deviceId}` | Get alert history |
| POST | `/api/monitoring/alerts/{alertId}/acknowledge` | Acknowledge alert |
| POST | `/api/monitoring/rules` | Create monitoring rule |
| GET | `/api/monitoring/rules` | List monitoring rules |

### Diagnostics

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/diagnostics/health` | Get system health |
| GET | `/api/diagnostics/logs?limit=50` | Get error logs |

### Workflows

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/workflows/complete` | Execute complete workflow |
| GET | `/api/workflows/device-health-report` | Get device health report |

---

## Example Workflows

### 1. Complete End-to-End Workflow

This workflow demonstrates the entire system:

**Request:**
```bash
curl -X POST http://localhost:5000/api/workflows/complete \
  -H "Content-Type: application/json" \
  -d '{"deviceId": "GATE-DEMO-001"}'
```

**What happens:**
1. Registers device `GATE-DEMO-001`
2. Creates monitoring rule for the device
3. Simulates device going offline
4. Waits for alert to be triggered (event-driven!)
5. Acknowledges the alert

**Response:**
```json
{
  "success": true,
  "message": "Complete workflow executed successfully for device GATE-DEMO-001",
  "steps": [
    {
      "stepNumber": 1,
      "stepName": "Register Device",
      "success": true,
      "result": "Registered device GATE-DEMO-001 of type GATE",
      "duration": "00:00:00.523"
    },
    {
      "stepNumber": 2,
      "stepName": "Create Monitoring Rule",
      "success": true,
      "result": "Created monitoring rule abc-123 for device GATE-DEMO-001",
      "duration": "00:00:00.312"
    },
    // ... more steps
  ]
}
```

### 2. Register Device and Monitor

**Step 1: Register Device**
```bash
curl -X POST http://localhost:5000/api/devices \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "LIFT-001",
    "deviceType": "LIFT",
    "location": "Building A - Floor 5",
    "metadata": "{\"capacity\": 10, \"floor\": 5}"
  }'
```

**Step 2: Create Monitoring Rule**
```bash
curl -X POST http://localhost:5000/api/monitoring/rules \
  -H "Content-Type: application/json" \
  -d '{
    "ruleName": "Monitor LIFT-001",
    "condition": "DEVICE_OFFLINE",
    "action": "CREATE_ALERT",
    "severity": "HIGH",
    "deviceIdFilter": "LIFT-001",
    "deviceTypeFilter": null
  }'
```

**Step 3: Simulate Device Event**
```bash
curl -X POST http://localhost:5000/api/devices/LIFT-001/simulate \
  -H "Content-Type: application/json" \
  -d '{"status": "OFFLINE"}'
```

**Step 4: Check Active Alerts**
```bash
curl http://localhost:5000/api/monitoring/alerts/active
```

### 3. Get Device Health Report

```bash
curl http://localhost:5000/api/workflows/device-health-report
```

**Response:**
```json
{
  "generatedAt": "2025-01-17T10:30:00Z",
  "totalDevices": 5,
  "healthyDevices": 3,
  "unhealthyDevices": 1,
  "offlineDevices": 1,
  "deviceSummaries": [
    {
      "deviceId": "GATE-001",
      "deviceType": "GATE",
      "status": "Active",
      "isOnline": true,
      "activeAlertCount": 0,
      "lastHeartbeat": "2025-01-17T10:29:45Z"
    },
    // ... more devices
  ]
}
```

---

## Configuration

### appsettings.json

```json
{
  "Services": {
    "DeviceService": "http://localhost:5001",
    "MonitoringService": "http://localhost:5002",
    "DiagnosticsService": "http://localhost:5003"
  }
}
```

### Docker Environment Variables

```yaml
environment:
  Services__DeviceService: "http://device-service:8080"
  Services__MonitoringService: "http://monitoring-service:8080"
  Services__DiagnosticsService: "http://diagnostics-service:8080"
```

---

## Testing with Swagger UI

1. Open http://localhost:5000
2. Swagger UI shows all available endpoints
3. Click "Try it out" on any endpoint
4. Fill in parameters
5. Click "Execute"
6. View response

### Try this sequence:

1. **POST /api/workflows/complete** - Execute full workflow
2. **GET /api/devices** - See all registered devices
3. **GET /api/monitoring/alerts/active** - See triggered alerts
4. **GET /api/workflows/device-health-report** - See overall health

---

## Error Handling

All errors are returned with proper HTTP status codes:

- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no response body
- `400 Bad Request` - Invalid input or operation failed
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Unexpected error

**Error Response Format:**
```json
{
  "error": "Failed to register device: Device with ID GATE-001 already exists"
}
```

---

## Key Patterns Demonstrated

### 1. Service Orchestration
The orchestrator coordinates multiple microservices to complete complex workflows.

### 2. gRPC to REST Translation
Converts REST API calls to gRPC calls and back.

### 3. Error Handling
Catches gRPC exceptions and converts to appropriate HTTP responses.

### 4. Dependency Injection
All gRPC clients are auto-injected:

```csharp
builder.Services.AddGrpcClient<DeviceService.DeviceServiceClient>(options =>
{
    options.Address = new Uri(config["Services:DeviceService"]);
});
```

### 5. Async/Await Pattern
All operations are fully asynchronous.

---

## Development

### Adding New Endpoints

1. **Add model** in `Models/`
2. **Add interface method** in `Services/I*Orchestrator.cs`
3. **Implement method** in `Services/*Orchestrator.cs`
4. **Add controller action** in `Controllers/*Controller.cs`
5. **Test in Swagger UI**

### Testing

```bash
# Unit tests
dotnet test

# Integration tests (requires services running)
dotnet test --filter Category=Integration
```

---

## Production Considerations

When deploying to production:

1. **Add Authentication** - JWT tokens, API keys
2. **Add Rate Limiting** - Prevent abuse
3. **Add Caching** - Redis for frequent queries
4. **Add Resilience** - Polly for retries, circuit breakers
5. **Add Logging** - Structured logging with Serilog
6. **Add Monitoring** - OpenTelemetry, Application Insights
7. **Add API Gateway** - Kong, Ocelot, YARP
8. **Add API Versioning** - Support multiple versions

---

## Learn More

- [QUICKSTART.md](../../../QUICKSTART.md) - Workshop quick start
- [PROGRESS.md](../../../PROGRESS.md) - Project progress
- [DeviceService README](../../Services/DeviceService/README.md)
- [MonitoringService README](../../Services/MonitoringService/README.md)
- [DiagnosticsService README](../../Services/DiagnosticsService/README.md)
