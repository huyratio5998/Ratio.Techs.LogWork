using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ratio.LogWork.Configurations;
using Ratio.LogWork.Service;
using Serilog;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try {
    // Create service collection
    var services = new ServiceCollection();

    // Add Serilog for logging
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog();
    });

    // Register configuration
    services.AddSingleton<IConfiguration>(configuration);

    // Register services using the extension method
    services.AddWorkLogServices(configuration);

    // Build service provider
    var serviceProvider = services.BuildServiceProvider();

    // Example usage of repositories
    using (var scope = serviceProvider.CreateScope())
    {
        var appService = scope.ServiceProvider.GetRequiredService<IApplicationService>();

        try
        {
            await appService.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
finally
{
    // End of the program
    Log.CloseAndFlush();
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

