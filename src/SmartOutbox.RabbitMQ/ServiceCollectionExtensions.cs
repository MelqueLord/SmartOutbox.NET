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
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHealthChecks().AddCheck<RabbitMqHealthCheck>("rabbitmq");
            return services;
        }
    }
}
