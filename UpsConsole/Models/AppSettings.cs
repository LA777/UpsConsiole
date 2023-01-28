namespace UpsConsole.Models
{
    public class AppSettings
    {
        public string Ip { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int SshPort { get; set; }
        public string SshTtl { get; set; } = null!;
    }
}