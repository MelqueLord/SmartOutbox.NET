using FluentAssertions;
using SmartOutbox.Core.Events;
using SmartOutbox.Core.Services;

namespace SmartOutbox.UnitTests;

public sealed class JsonSerializerServiceTests
{
    [Fact]
    public void Serialize_UsesCamelCasePayload()
    {
        var serializer = new JsonSerializerService();
        IntegrationEvent integrationEvent = new OrderCreatedEvent(Guid.Parse("d658a267-e828-475c-9b27-a67fd053df72"), "Acme", 99.95m);

        var payload = serializer.Serialize(integrationEvent);

        payload.Should().Contain("\"orderId\"");
        payload.Should().Contain("\"customerName\":\"Acme\"");
        payload.Should().Contain("\"amount\":99.95");
    }
}
