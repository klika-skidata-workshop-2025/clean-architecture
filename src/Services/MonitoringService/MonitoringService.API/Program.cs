using MonitoringService.API.Services;
using MonitoringService.Application;
using MonitoringService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// gRPC services
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// gRPC reflection (for tools like grpcurl)
builder.Services.AddGrpcReflection();

// Health checks
builder.Services.AddGrpcHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Ensure database is created and migrated
await app.Services.EnsureDatabaseAsync();

// Configure the HTTP request pipeline
app.MapGrpcService<MonitoringGrpcService>();

// Map health check service
app.MapGrpcHealthChecksService();

// Map gRPC reflection service (development only)
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086032");

await app.RunAsync();
