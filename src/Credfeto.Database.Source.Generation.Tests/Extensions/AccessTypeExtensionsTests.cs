using System;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Models;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests.Extensions;

public sealed class AccessTypeExtensionsTests : TestBase
{
    [Fact]
    public void PublicAccessTypeReturnsPublicKeyword()
    {
        string result = AccessType.PUBLIC.ToKeywords();
        Assert.Equal(expected: "public", actual: result);
    }

    [Fact]
    public void PrivateAccessTypeReturnsPrivateKeyword()
    {
        string result = AccessType.PRIVATE.ToKeywords();
        Assert.Equal(expected: "private", actual: result);
    }

    [Fact]
    public void ProtectedAccessTypeReturnsProtectedKeyword()
    {
        string result = AccessType.PROTECTED.ToKeywords();
        Assert.Equal(expected: "protected", actual: result);
    }

    [Fact]
    public void ProtectedInternalAccessTypeReturnsProtectedInternalKeywords()
    {
        string result = AccessType.PROTECTED_INTERNAL.ToKeywords();
        Assert.Equal(expected: "protected internal", actual: result);
    }

    [Fact]
    public void InternalAccessTypeReturnsInternalKeyword()
    {
        string result = AccessType.INTERNAL.ToKeywords();
        Assert.Equal(expected: "internal", actual: result);
    }

    [Fact]
    public void InvalidAccessTypeThrowsArgumentOutOfRangeException()
    {
        // Cast an out-of-range value to AccessType to trigger the _ => throw branch.
        // This covers the default case in the switch expression in AccessTypeExtensions.cs.
        const AccessType invalidValue = (AccessType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => invalidValue.ToKeywords());
    }
}
