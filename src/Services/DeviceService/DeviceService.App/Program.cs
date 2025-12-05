using DeviceService.Service;

Console.WriteLine("========================================");
Console.WriteLine("DeviceService - Console Host");
Console.WriteLine("========================================");
Console.WriteLine();

try
{
    // Create and configure the service
    var host = ServiceHostBuilder
        .Create(args)
        .Build();

    Console.WriteLine("Starting DeviceService...");
    Console.WriteLine();

    // Check if running in interactive mode
    bool isInteractive = Environment.UserInteractive && !Console.IsInputRedirected;

    if (isInteractive)
    {
        // Start the service
        await host.StartAsync();

        Console.WriteLine("DeviceService started successfully!");
        Console.WriteLine($"Listening on: {string.Join(", ", host.Urls)}");
        Console.WriteLine();
        Console.WriteLine("Device Management Features:");
        Console.WriteLine("  - Register new devices (gates, lifts, counters, controls)");
        Console.WriteLine("  - Update device status and information");
        Console.WriteLine("  - Monitor device heartbeats");
        Console.WriteLine("  - Simulate device events");
        Console.WriteLine();
        Console.WriteLine("Press ENTER to stop the service...");
        Console.WriteLine();

        // Wait for user to press ENTER
        Console.ReadLine();

        Console.WriteLine();
        Console.WriteLine("Stopping DeviceService...");

        // Stop the service gracefully
        await host.StopAsync();

        Console.WriteLine("DeviceService stopped.");
    }
    else
    {
        // Non-interactive mode - run until cancelled
        Console.WriteLine("Running in non-interactive mode...");
        await host.RunAsync();
    }
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
