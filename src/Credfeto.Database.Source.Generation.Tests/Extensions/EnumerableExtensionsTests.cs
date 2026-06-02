using System.Collections.Generic;
using Credfeto.Database.Source.Generation.Extensions;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests.Extensions;

public sealed class EnumerableExtensionsTests : TestBase
{
    [Fact]
    public void RemoveNullsFromReferenceTypeEnumerableRemovesNullValues()
    {
        string?[] source = ["a", null, "b", null, "c"];
        IReadOnlyList<string> result = [.. source.RemoveNulls()];
        Assert.Equal(expected: 3, actual: result.Count);
        Assert.Equal(expected: "a", actual: result[0]);
        Assert.Equal(expected: "b", actual: result[1]);
        Assert.Equal(expected: "c", actual: result[2]);
    }

    [Fact]
    public void RemoveNullsFromReferenceTypeEnumerableWithAllNullsReturnsEmpty()
    {
        string?[] source = [null, null, null];
        IReadOnlyList<string> result = [.. source.RemoveNulls()];
        Assert.Empty(result);
    }

    [Fact]
    public void RemoveNullsFromReferenceTypeEnumerableWithNoNullsReturnsAll()
    {
        string?[] source = ["a", "b", "c"];
        IReadOnlyList<string> result = [.. source.RemoveNulls()];
        Assert.Equal(expected: 3, actual: result.Count);
    }

    [Fact]
    public void RemoveNullsFromReferenceTypeEnumerableWithEmptySourceReturnsEmpty()
    {
        string?[] source = [];
        IReadOnlyList<string> result = [.. source.RemoveNulls()];
        Assert.Empty(result);
    }

    [Fact]
    public void RemoveNullsFromValueTypeEnumerableRemovesNullValues()
    {
        int?[] source = [1, null, 2, null, 3];
        IReadOnlyList<int> result = [.. source.RemoveNulls()];
        Assert.Equal(expected: 3, actual: result.Count);
        Assert.Equal(expected: 1, actual: result[0]);
        Assert.Equal(expected: 2, actual: result[1]);
        Assert.Equal(expected: 3, actual: result[2]);
    }

    [Fact]
    public void RemoveNullsFromValueTypeEnumerableWithAllNullsReturnsEmpty()
    {
        int?[] source = [null, null, null];
        IReadOnlyList<int> result = [.. source.RemoveNulls()];
        Assert.Empty(result);
    }

    [Fact]
    public void RemoveNullsFromValueTypeEnumerableWithNoNullsReturnsAll()
    {
        int?[] source = [1, 2, 3];
        IReadOnlyList<int> result = [.. source.RemoveNulls()];
        Assert.Equal(expected: 3, actual: result.Count);
    }

    [Fact]
    public void RemoveNullsFromValueTypeEnumerableWithEmptySourceReturnsEmpty()
    {
        int?[] source = [];
        IReadOnlyList<int> result = [.. source.RemoveNulls()];
        Assert.Empty(result);
    }
}
