using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartOutbox.RabbitMQ.HealthChecks;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartOutboxRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection("RabbitMq"))
                .Validate(options => !string.IsNullOrWhiteSpace(options.HostName), "RabbitMq:HostName is required.")
                .Validate(options => options.Port > 0, "RabbitMq:Port must be greater than zero.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "RabbitMq:UserName is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ExchangeName), "RabbitMq:ExchangeName is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.QueueName), "RabbitMq:QueueName is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.DeadLetterExchangeName), "RabbitMq:DeadLetterExchangeName is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.DeadLetterQueueName), "RabbitMq:DeadLetterQueueName is required.")
                .ValidateOnStart();
            services.AddSingleton<IRabbitMqClient, DefaultRabbitMqClient>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq");
            return services;
        }
    }
}
