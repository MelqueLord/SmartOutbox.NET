using System.Text.Json;
using SmartOutbox.Core.Interfaces;

namespace SmartOutbox.Core.Services
{
    public sealed class JsonSerializerService : IJsonSerializerService
    {
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public string Serialize<T>(T value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return JsonSerializer.Serialize(value, value.GetType(), _options);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonSerializer.Deserialize<T>(payload, _options)
                ?? throw new JsonException($"Unable to deserialize payload to {typeof(T).Name}.");
        }
    }
}
