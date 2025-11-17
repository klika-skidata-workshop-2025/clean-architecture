using ServiceTemplate.Application;
using ServiceTemplate.Infrastructure;

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
app.MapGrpcService<GreeterService>();

// Map health check service
app.MapGrpcHealthChecksService();

// Map gRPC reflection service (development only)
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086032");

await app.RunAsync();

// TODO: Replace this with your actual gRPC service implementation
public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, Grpc.Core.ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}

public class HelloRequest
{
    public string Name { get; set; } = string.Empty;
}

public class HelloReply
{
    public string Message { get; set; } = string.Empty;
}

public class Greeter
{
    public class GreeterBase
    {
        public virtual Task<HelloReply> SayHello(HelloRequest request, Grpc.Core.ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}
