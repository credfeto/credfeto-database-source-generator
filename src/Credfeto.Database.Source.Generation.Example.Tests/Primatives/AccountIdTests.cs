using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Primatives;

public sealed class AccountIdTests : TestBase
{
    [Theory]
    [InlineData(1L)]
    [InlineData(42L)]
    [InlineData(long.MaxValue)]
    public void AccountId_WithValue_HasExpectedValue(long expectedValue)
    {
        AccountId accountId = new(expectedValue);

        Assert.Equal(expectedValue, accountId.Value);
    }

    [Fact]
    public void AccountId_WithSameValue_AreEqual()
    {
        AccountId first = new(42L);
        AccountId second = new(42L);

        Assert.Equal(first, second);
    }

    [Fact]
    public void AccountId_WithDifferentValues_AreNotEqual()
    {
        AccountId first = new(1L);
        AccountId second = new(2L);

        Assert.NotEqual(first, second);
    }
}
