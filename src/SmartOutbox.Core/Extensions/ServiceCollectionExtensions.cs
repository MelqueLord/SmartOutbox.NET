using Microsoft.Extensions.DependencyInjection;
using SmartOutbox.Core.Consumption;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Interfaces;
using SmartOutbox.Core.Services;

namespace SmartOutbox.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartOutboxCore(this IServiceCollection services)
        {
            services.AddSingleton<IJsonSerializerService, JsonSerializerService>();
            services.AddSingleton<ICorrelationContext, CorrelationContext>();
            services.AddScoped(typeof(IntegrationEventConsumer<>));
            return services;
        }

        public static IServiceCollection AddIntegrationEventHandler<TIntegrationEvent, THandler>(this IServiceCollection services)
            where TIntegrationEvent : IntegrationEvent
            where THandler : class, IIntegrationEventHandler<TIntegrationEvent>
        {
            services.AddScoped<IIntegrationEventHandler<TIntegrationEvent>, THandler>();
            return services;
        }

        public static IServiceCollection AddInMemoryProcessedMessageStore(this IServiceCollection services)
        {
            services.AddSingleton<IProcessedMessageStore, InMemoryProcessedMessageStore>();
            return services;
        }
    }
}
