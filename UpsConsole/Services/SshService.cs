using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using UpsConsole.Models;

namespace UpsConsole.Services
{
    public class SshService : ISshService
    {
        private static ILogger<SshService> _logger = null!;
        private readonly AppSettings _appSettings;

        public SshService(ILogger<SshService> logger, IOptionsMonitor<AppSettings> optsMonitor)
        {
            var optsMonitor1 = optsMonitor ?? throw new ArgumentNullException(nameof(optsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appSettings = optsMonitor1.CurrentValue;
        }

        public void ShutdownDeviceSsh()
        {
            _logger.LogInformation("ShutdownDeviceSsh");
            using var client = new SshClient(_appSettings.Ip, _appSettings.SshPort, _appSettings.UserName, _appSettings.SshTtl);
            client.ErrorOccurred += SshClientErrorOccurred;
            try
            {
                client.Connect();
                _logger.LogInformation(client.ConnectionInfo.ServerVersion);
                _logger.LogInformation($"Shutdown remote PC - {_appSettings.Ip}");
                client.RunCommand("shutdown /s");
                _logger.LogInformation("Shutdown PC completed.");
                client.Disconnect();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.Message);
                throw;
            }
        }

        private static void SshClientErrorOccurred(object? sender, ExceptionEventArgs exceptionEventArgs)
        {
            _logger.LogInformation("An ssh error occurred:");
            _logger.LogInformation(exceptionEventArgs.Exception.ToString());
        }
    }
}