namespace PolicyEventHub.Infrastructure.Configurations
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = null!;
        public ushort Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Exchange { get; set; } = null!;
    }
}
