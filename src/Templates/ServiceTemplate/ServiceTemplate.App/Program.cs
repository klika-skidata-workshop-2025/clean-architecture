using ServiceTemplate.Service;

Console.WriteLine("========================================");
Console.WriteLine("ServiceTemplate - Console Host");
Console.WriteLine("========================================");
Console.WriteLine();

try
{
    // Create and configure the service
    var host = ServiceHostBuilder
        .Create(args)
        .Build();

    Console.WriteLine("Starting service...");
    Console.WriteLine();

    // Start the service
    await host.StartAsync();

    Console.WriteLine("Service started successfully!");
    Console.WriteLine($"Listening on: {string.Join(", ", host.Urls)}");
    Console.WriteLine();
    Console.WriteLine("Press ENTER to stop the service...");
    Console.WriteLine();

    // Wait for user to press ENTER
    Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine("Stopping service...");

    // Stop the service gracefully
    await host.StopAsync();

    Console.WriteLine("Service stopped.");
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
