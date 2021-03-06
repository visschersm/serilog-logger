using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var host = AppStartup();

var service = ActivatorUtilities.CreateInstance<Foo>(host.Services);

service.DoLog();

static void BuildConfig(IConfigurationBuilder builder)
{
    // Check the current directory that the application is running on 
    // Then once the file 'appsetting.json' is found, we are adding it.
    // We add env variables, which can override the configs in appsettings.json
    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
}

static IHost AppStartup()
{
    var builder = new ConfigurationBuilder();
    BuildConfig(builder);

    // Specifying the configuration for serilog
    Log.Logger = new LoggerConfiguration() // initiate the logger configuration
        .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
        .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
        .WriteTo.Console() // decide where the logs are going to be shown
        .CreateLogger(); //initialise the logger

    Log.Logger.Information("Application Starting");

    var host = Host.CreateDefaultBuilder() // Initialising the Host 
        .ConfigureServices((context, services) => 
        { // Adding the DI container for configuration
            services.AddScoped<Foo>();
        })
        .UseSerilog() // Add Serilog
        .Build(); // Build the Host

    return host;
}

public class Foo
{
    private readonly ILogger<Foo> logger;

    public Foo(ILogger<Foo> logger) => this.logger = logger;

    public void DoLog()
    {
        logger.LogInformation("Hello log");
    }
}