using Workshop.Orchestrator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Workshop Orchestrator API",
        Version = "v1",
        Description = "REST API that orchestrates calls to Device, Monitoring, and Diagnostics microservices"
    });
});

// Register gRPC clients
builder.Services.AddGrpcClient<Workshop.Contracts.Device.DeviceService.DeviceServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:DeviceService"] ?? "http://localhost:5001");
});

builder.Services.AddGrpcClient<Workshop.Contracts.Monitoring.MonitoringService.MonitoringServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:MonitoringService"] ?? "http://localhost:5002");
});

builder.Services.AddGrpcClient<Workshop.Contracts.Diagnostics.DiagnosticsService.DiagnosticsServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:DiagnosticsService"] ?? "http://localhost:5003");
});

// Register orchestrator services
builder.Services.AddScoped<IDeviceOrchestrator, DeviceOrchestrator>();
builder.Services.AddScoped<IMonitoringOrchestrator, MonitoringOrchestrator>();
builder.Services.AddScoped<IDiagnosticsOrchestrator, DiagnosticsOrchestrator>();
builder.Services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workshop Orchestrator API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
