using MonitoringService.Service;

Console.WriteLine("========================================");
Console.WriteLine("MonitoringService - Console Host");
Console.WriteLine("========================================");
Console.WriteLine();

try
{
    var host = ServiceHostBuilder
        .Create(args)
        .Build();

    Console.WriteLine("Starting MonitoringService...");
    Console.WriteLine();

    await host.StartAsync();

    Console.WriteLine("MonitoringService started successfully!");
    Console.WriteLine($"Listening on: {string.Join(", ", host.Urls)}");
    Console.WriteLine();
    Console.WriteLine("Monitoring Features:");
    Console.WriteLine("  - Alert management and acknowledgment");
    Console.WriteLine("  - Monitoring rule creation and evaluation");
    Console.WriteLine("  - Device event processing");
    Console.WriteLine("  - Automatic alert triggering");
    Console.WriteLine();
    Console.WriteLine("Press ENTER to stop the service...");
    Console.WriteLine();

    Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine("Stopping MonitoringService...");

    await host.StopAsync();

    Console.WriteLine("MonitoringService stopped.");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("FATAL ERROR:");
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    Console.WriteLine("Stack Trace:");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;
