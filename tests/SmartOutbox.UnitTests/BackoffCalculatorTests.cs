using FluentAssertions;
using Microsoft.Extensions.Options;
using SmartOutbox.Worker.Options;
using SmartOutbox.Worker.Services;

namespace SmartOutbox.UnitTests;

public sealed class BackoffCalculatorTests
{
    [Fact]
    public void CalculateDelay_UsesExponentialBackoffWithCap()
    {
        var calculator = new BackoffCalculator(Options.Create(new OutboxProcessorOptions
        {
            BackoffBaseSeconds = 5,
            MaxBackoffSeconds = 20
        }));

        calculator.CalculateDelay(1).Should().Be(TimeSpan.FromSeconds(5));
        calculator.CalculateDelay(2).Should().Be(TimeSpan.FromSeconds(10));
        calculator.CalculateDelay(3).Should().Be(TimeSpan.FromSeconds(20));
        calculator.CalculateDelay(4).Should().Be(TimeSpan.FromSeconds(20));
    }
}
