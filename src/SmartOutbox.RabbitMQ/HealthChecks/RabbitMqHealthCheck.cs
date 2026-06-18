using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartOutbox.RabbitMQ.Options;

namespace SmartOutbox.RabbitMQ.HealthChecks
{
    public sealed class RabbitMqHealthCheck : IHealthCheck
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqHealthCheck(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
                };
                await using var connection = await factory.CreateConnectionAsync(cancellationToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy("RabbitMQ is available.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ check failed.", ex);
            }
        }
    }
}
