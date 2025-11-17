using DeviceService.Service;

// Create and configure the service for Windows Service hosting
var host = ServiceHostBuilder
    .Create(args)
    .ConfigureWindowsService()
    .Build();

// Run the service
await host.RunAsync();
