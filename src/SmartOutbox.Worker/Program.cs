using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartOutbox.Core.Extensions;
using SmartOutbox.EntityFramework;
using SmartOutbox.Worker.Options;
using SmartOutbox.Worker.Services;
using SmartOutbox.RabbitMQ;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSmartOutboxCore();
        services.AddSmartOutboxEntityFramework(context.Configuration);
        services.AddSmartOutboxRabbitMq(context.Configuration);
        services.Configure<OutboxProcessorOptions>(context.Configuration.GetSection("OutboxProcessor"));
        services.AddHostedService<OutboxProcessorService>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
