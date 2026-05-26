namespace SmartOutbox.RabbitMQ.Options
{
    public sealed class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "outbox.events";
        public string ExchangeType { get; set; } = "topic";
        public string VirtualHost { get; set; } = "/";
    }
}
