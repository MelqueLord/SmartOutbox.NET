using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartOutbox.SampleApi.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context.Request.Headers);
            context.Items[HeaderName] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            using (_logger.BeginScope(new Dictionary<string, object> { [HeaderName] = correlationId }))
            {
                await _next(context);
            }
        }

        private static string GetOrCreateCorrelationId(IHeaderDictionary headers)
        {
            if (headers.TryGetValue(HeaderName, out var values) && !string.IsNullOrWhiteSpace(values))
            {
                return values.ToString();
            }

            return Guid.NewGuid().ToString("D");
        }
    }
}
