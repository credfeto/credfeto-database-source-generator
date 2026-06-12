using System;
using Credfeto.Database.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Tests.Helpers;

public sealed class RetryDelayCalculatorTests : TestBase
{
    [Fact]
    public void Calculate_WithAttemptOne_ReturnsZero()
    {
        TimeSpan result = RetryDelayCalculator.Calculate(attempts: 1);

        Assert.Equal(expected: TimeSpan.Zero, actual: result);
    }

    [Fact]
    public void Calculate_WithAttemptTwo_ReturnsFourSeconds()
    {
        TimeSpan result = RetryDelayCalculator.Calculate(attempts: 2);

        Assert.Equal(expected: TimeSpan.FromSeconds(4), actual: result);
    }

    [Fact]
    public void Calculate_WithAttemptThree_ReturnsEightSeconds()
    {
        TimeSpan result = RetryDelayCalculator.Calculate(attempts: 3);

        Assert.Equal(expected: TimeSpan.FromSeconds(8), actual: result);
    }

    [Fact]
    public void CalculateWithJitter_WithAttemptOne_ReturnsZero()
    {
        TimeSpan result = RetryDelayCalculator.CalculateWithJitter(attempts: 1, maxJitterSeconds: 2);

        Assert.Equal(expected: TimeSpan.Zero, actual: result);
    }

    [Fact]
    public void CalculateWithJitter_WithAttemptTwo_WhenJitterSmallerThanDelay_ReturnsNonNegativeValue()
    {
        TimeSpan result = RetryDelayCalculator.CalculateWithJitter(attempts: 2, maxJitterSeconds: 1);

        Assert.True(result >= TimeSpan.Zero, userMessage: "Delay with jitter should be non-negative");
    }

    [Fact]
    public void CalculateWithJitter_WithAttemptTwo_WhenJitterLargerThanDelay_ReturnsNonNegativeValue()
    {
        TimeSpan result = RetryDelayCalculator.CalculateWithJitter(attempts: 2, maxJitterSeconds: 10);

        Assert.True(result >= TimeSpan.Zero, userMessage: "Delay with large jitter should be non-negative");
    }
}
