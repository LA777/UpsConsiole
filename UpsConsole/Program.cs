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
            Console.WriteLine("App started - version 1.0.2");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Set working directory if in production
                    if (!hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                    }

                    var configuration = hostContext.Configuration;
                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

                    // Add Serilog support
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .Enrich.FromLogContext()
                        .MinimumLevel.Verbose()
                        .WriteTo.Console()
                        .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
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