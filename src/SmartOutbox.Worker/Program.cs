using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartOutbox.Core.Extensions;
using SmartOutbox.EntityFramework;
using SmartOutbox.Worker.Options;
using SmartOutbox.Worker.Services;
using SmartOutbox.RabbitMQ;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
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
        services.AddOptions<OutboxProcessorOptions>()
            .Bind(context.Configuration.GetSection("OutboxProcessor"))
            .Validate(options => options.PollingIntervalSeconds > 0, "PollingIntervalSeconds must be greater than zero.")
            .Validate(options => options.MaxRetryCount > 0, "MaxRetryCount must be greater than zero.")
            .Validate(options => options.BatchSize > 0, "BatchSize must be greater than zero.")
            .Validate(options => options.BackoffBaseSeconds > 0, "BackoffBaseSeconds must be greater than zero.")
            .Validate(options => options.MaxBackoffSeconds >= options.BackoffBaseSeconds, "MaxBackoffSeconds must be greater than or equal to BackoffBaseSeconds.")
            .ValidateOnStart();
        services.AddSingleton<IBackoffCalculator, BackoffCalculator>();
        services.AddHostedService<OutboxProcessorService>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
