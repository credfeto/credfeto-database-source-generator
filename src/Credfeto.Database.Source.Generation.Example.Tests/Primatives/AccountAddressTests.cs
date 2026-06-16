using System;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Primatives;

public sealed class AccountAddressTests : TestBase
{
    [Fact]
    public void AccountAddress_WithValue_HasExpectedValue()
    {
        const string EXPECTED_VALUE = "0x1234567890abcdef";

        AccountAddress address = new() { Value = EXPECTED_VALUE };

        Assert.Equal(EXPECTED_VALUE, address.Value, StringComparer.Ordinal);
    }
}
