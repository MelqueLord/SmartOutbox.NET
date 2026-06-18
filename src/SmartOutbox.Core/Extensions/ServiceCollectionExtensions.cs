using Microsoft.Extensions.DependencyInjection;
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
            return services;
        }
    }
}
