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
        public string QueueName { get; set; } = "smartoutbox.events";
        public string DeadLetterExchangeName { get; set; } = "outbox.events.dlx";
        public string DeadLetterQueueName { get; set; } = "smartoutbox.events.dlq";
        public string RoutingKey { get; set; } = "#";
        public string VirtualHost { get; set; } = "/";
    }
}
