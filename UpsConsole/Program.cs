using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Runtime.Versioning;
using UpsConsole.Models;
using UpsConsole.Services;

namespace UpsConsole
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("App started - version 1.0.5");
            CreateHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    configuration
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile("appsettings.{Environment.GetEnvironmentVariable(\"ASPNETCORE_ENVIRONMENT\")}.json", true, true);

                    configuration.AddEnvironmentVariables();

                    if (args is { Length: > 0 })
                    {
                        configuration.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Set working directory if in production
                    if (!hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                    }

                    var configuration = hostContext.Configuration;
                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                    services.AddSingleton(configuration);
                    services.AddHostedService<EventsWorker>();
                    services.AddSingleton<ITaskService, TaskService>();
                    services.AddSingleton<ISshService, SshService>();
                    services.AddSingleton<IConsoleSpinner, ConsoleSpinner>();
                    services.AddSingleton<IWakeOnlineService, WakeOnlineService>();
                })
                .UseSerilog()
                .UseWindowsService();
        }
    }
}