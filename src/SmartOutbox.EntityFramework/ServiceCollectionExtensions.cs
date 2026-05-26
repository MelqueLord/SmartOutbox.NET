using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartOutbox.Core.Interfaces;

namespace SmartOutbox.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartOutboxEntityFramework(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), builder =>
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
            });

            services.AddScoped<IEventPublisher, EfEventPublisher>();
            return services;
        }
    }
}
