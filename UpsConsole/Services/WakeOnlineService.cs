using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UpsConsole.Models;

namespace UpsConsole.Services
{
    public class WakeOnlineService : IWakeOnlineService
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<WakeOnlineService> _logger;

        public WakeOnlineService(ILogger<WakeOnlineService> logger, IOptionsMonitor<AppSettings> optsMonitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var optsMonitor1 = optsMonitor ?? throw new ArgumentNullException(nameof(optsMonitor));
            _appSettings = optsMonitor1.CurrentValue;
        }

        public void WakeOnLan()
        {
            _logger.LogInformation("Waking PC...");
            var attempts = 5;
            var sleepTimeMs = 100;

            for (var i = 0; i < attempts; i++)
            {
                _logger.LogInformation($"Attempt {i + 1}/{attempts}: ");
                WakeDevice(_appSettings.Mac).GetAwaiter().GetResult();
                _logger.LogInformation("WOL package send.");
                Thread.Sleep(sleepTimeMs);
            }

            _logger.LogInformation("Waking completed.");
        }

        private async Task WakeDevice(string macAddress)
        {
            var magicPacket = BuildMagicPacket(macAddress);
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces().Where(n =>
                         n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up))
            {
                var iPInterfaceProperties = networkInterface.GetIPProperties();
                foreach (var multicastIpAddressInformation in iPInterfaceProperties.MulticastAddresses)
                {
                    var multicastIpAddress = multicastIpAddressInformation.Address;
                    if (multicastIpAddress.ToString().StartsWith("ff02::1%", StringComparison.OrdinalIgnoreCase)) // Ipv6: All hosts on LAN (with zone index)
                    {
                        var unicastIpAddressInformation = iPInterfaceProperties.UnicastAddresses.Where(u =>
                            u.Address.AddressFamily == AddressFamily.InterNetworkV6 && !u.Address.IsIPv6LinkLocal).FirstOrDefault();
                        if (unicastIpAddressInformation == null)
                        {
                            continue;
                        }

                        await SendWakeOnLan(unicastIpAddressInformation.Address, multicastIpAddress, magicPacket);
                        break;
                    }

                    if (multicastIpAddress.ToString().Equals("224.0.0.1")) // Ipv4: All hosts on LAN
                    {
                        var unicastIpAddressInformation = iPInterfaceProperties.UnicastAddresses.Where(u =>
                            u.Address.AddressFamily == AddressFamily.InterNetwork && !iPInterfaceProperties.GetIPv4Properties().IsAutomaticPrivateAddressingActive).FirstOrDefault();
                        if (unicastIpAddressInformation == null)
                        {
                            continue;
                        }

                        await SendWakeOnLan(unicastIpAddressInformation.Address, multicastIpAddress, magicPacket);
                        break;
                    }
                }
            }
        }

        private byte[] BuildMagicPacket(string macAddress) // MacAddress in any standard HEX format
        {
            macAddress = Regex.Replace(macAddress, "[: -]", "");
            var macBytes = new byte[6];
            for (var i = 0; i < 6; i++)
            {
                macBytes[i] = Convert.ToByte(macAddress.Substring(i * 2, 2), 16);
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            for (var i = 0; i < 6; i++) //First 6 times 0xff
            {
                bw.Write((byte)0xff);
            }

            for (var i = 0; i < 16; i++) // then 16 times MacAddress
            {
                bw.Write(macBytes);
            }


            return ms.ToArray(); // 102 bytes magic packet
        }

        private async Task SendWakeOnLan(IPAddress localIpAddress, IPAddress ipAddress, byte[] magicPacket)
        {
            using var client = new UdpClient(new IPEndPoint(localIpAddress, 0));
            await client.SendAsync(magicPacket, magicPacket.Length, ipAddress.ToString(), _appSettings.WolPort);
        }
    }
}