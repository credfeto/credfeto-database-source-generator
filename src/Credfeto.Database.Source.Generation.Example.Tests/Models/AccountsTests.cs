using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using FunFair.Test.Infrastructure.Mocks;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Models;

public sealed class AccountsTests : TestBase
{
    private static readonly AccountAddress SharedAddress = new() { Value = "0x1234567890abcdef" };

    private static readonly Accounts ZeroObject = new(
        Id: 0,
        Name: string.Empty,
        Address: SharedAddress,
        LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
    );

    private static readonly Accounts Value1 = new(
        Id: 1,
        Name: "Test Account",
        Address: SharedAddress,
        LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
    );

    private static readonly Accounts EquivalentToValue1 = new(
        Id: 1,
        Name: "Test Account",
        Address: SharedAddress,
        LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
    );

    private static bool OperatorEquals(Accounts? x, Accounts? y)
    {
        return x == y;
    }

    private static bool OperatorNotEquals(Accounts? x, Accounts? y)
    {
        return x != y;
    }

    private static bool TypedEquals(Accounts x, Accounts? y)
    {
        return x.Equals(y);
    }

    private static bool UntypedEquals(Accounts x, object? y)
    {
        return x.Equals(y);
    }

    [Fact]
    public static void GetHashCodeSameNoMatterHowManyTimesCalled()
    {
        int first = Value1.GetHashCode();
        int second = Value1.GetHashCode();

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public static void GetHashCodeValue1ObjectIsSameAsEquivalentToValue1Object()
    {
        Assert.Equal(expected: Value1.GetHashCode(), actual: EquivalentToValue1.GetHashCode());
    }

    [Fact]
    public static void GetHashCodeValue1ObjectIsSameAsValue1AliasObject()
    {
        Accounts alias = Value1;

        Assert.Equal(expected: Value1.GetHashCode(), actual: alias.GetHashCode());
    }

    [Fact]
    public static void GetHashCodeValue1ObjectIsSameAsValue1Object()
    {
        Assert.Equal(expected: Value1.GetHashCode(), actual: Value1.GetHashCode());
    }

    [Fact]
    public static void GetHashCodeZeroObjectIsSameAsZeroObject()
    {
        Assert.Equal(expected: ZeroObject.GetHashCode(), actual: ZeroObject.GetHashCode());
    }

    [Fact]
    public static void OperatorEqualsNullObjectDifferentToZeroObject()
    {
        Assert.False(OperatorEquals(x: null, y: ZeroObject), userMessage: "null should not equal zero object");
    }

    [Fact]
    public static void OperatorEqualsNullObjectSameAsNullObject()
    {
        Assert.True(OperatorEquals(x: null, y: null), userMessage: "null should equal null");
    }

    [Fact]
    public static void OperatorEqualsValue1ObjectIsSameAsEquivalentToValue1Object()
    {
        Assert.True(OperatorEquals(x: Value1, y: EquivalentToValue1), userMessage: "value1 should equal equivalent");
    }

    [Fact]
    public static void OperatorEqualsValue1ObjectIsSameAsValue1AliasObject()
    {
        Accounts alias = Value1;

        Assert.True(OperatorEquals(x: Value1, y: alias), userMessage: "value1 should equal its alias");
    }

    [Fact]
    public static void OperatorEqualsValue1ObjectIsSameAsValue1Object()
    {
        Assert.True(OperatorEquals(x: Value1, y: Value1), userMessage: "value1 should equal itself");
    }

    [Fact]
    public static void OperatorEqualsZeroObjectDifferentToNullObject()
    {
        Assert.False(OperatorEquals(x: ZeroObject, y: null), userMessage: "zero object should not equal null");
    }

    [Fact]
    public static void OperatorEqualsZeroObjectIsSameAsZeroObject()
    {
        Assert.True(OperatorEquals(x: ZeroObject, y: ZeroObject), userMessage: "zero object should equal itself");
    }

    [Fact]
    public static void OperatorNotEqualsNullObjectDifferentToZeroObject()
    {
        Assert.True(OperatorNotEquals(x: null, y: ZeroObject), userMessage: "null should not equal zero object");
    }

    [Fact]
    public static void OperatorNotEqualsNullObjectSameAsNullObject()
    {
        Assert.False(OperatorNotEquals(x: null, y: null), userMessage: "null should equal null");
    }

    [Fact]
    public static void OperatorNotEqualsValue1ObjectIsSameAsEquivalentToValue1Object()
    {
        Assert.False(
            OperatorNotEquals(x: Value1, y: EquivalentToValue1),
            userMessage: "value1 should equal equivalent"
        );
    }

    [Fact]
    public static void OperatorNotEqualsValue1ObjectIsSameAsValue1AliasObject()
    {
        Accounts alias = Value1;

        Assert.False(OperatorNotEquals(x: Value1, y: alias), userMessage: "value1 should equal its alias");
    }

    [Fact]
    public static void OperatorNotEqualsValue1ObjectIsSameAsValue1Object()
    {
        Assert.False(OperatorNotEquals(x: Value1, y: Value1), userMessage: "value1 should equal itself");
    }

    [Fact]
    public static void OperatorNotEqualsZeroObjectDifferentToNullObject()
    {
        Assert.True(OperatorNotEquals(x: ZeroObject, y: null), userMessage: "zero object should not equal null");
    }

    [Fact]
    public static void OperatorNotEqualsZeroObjectIsSameAsZeroObject()
    {
        Assert.False(OperatorNotEquals(x: ZeroObject, y: ZeroObject), userMessage: "zero object should equal itself");
    }

    [Fact]
    public static void TypedEqualsValue1ObjectIsSameAsEquivalentToValue1Object()
    {
        Assert.True(TypedEquals(x: Value1, y: EquivalentToValue1), userMessage: "value1 should equal equivalent");
    }

    [Fact]
    public static void TypedEqualsValue1ObjectIsSameAsValue1AliasObject()
    {
        Accounts alias = Value1;

        Assert.True(TypedEquals(x: Value1, y: alias), userMessage: "value1 should equal its alias");
    }

    [Fact]
    public static void TypedEqualsValue1ObjectIsSameAsValue1Object()
    {
        Assert.True(TypedEquals(x: Value1, y: Value1), userMessage: "value1 should equal itself");
    }

    [Fact]
    public static void TypedEqualsZeroObjectDifferentToNullObject()
    {
        Assert.False(TypedEquals(x: ZeroObject, y: null), userMessage: "zero object should not equal null");
    }

    [Fact]
    public static void TypedEqualsZeroObjectIsSameAsZeroObject()
    {
        Assert.True(TypedEquals(x: ZeroObject, y: ZeroObject), userMessage: "zero object should equal itself");
    }

    [Fact]
    public static void UntypedEqualsValue1ObjectIsSameAsEquivalentToValue1Object()
    {
        Assert.True(UntypedEquals(x: Value1, y: EquivalentToValue1), userMessage: "value1 should equal equivalent");
    }

    [Fact]
    public static void UntypedEqualsValue1ObjectIsSameAsEquivalentToValue1ObjectAsObject()
    {
        object equivalent = EquivalentToValue1;

        Assert.True(UntypedEquals(x: Value1, y: equivalent), userMessage: "value1 should equal equivalent as object");
    }

    [Fact]
    public static void UntypedEqualsValue1ObjectIsSameAsValue1AliasObject()
    {
        object alias = Value1;

        Assert.True(UntypedEquals(x: Value1, y: alias), userMessage: "value1 should equal its alias");
    }

    [Fact]
    public static void UntypedEqualsValue1ObjectIsSameAsValue1Object()
    {
        Assert.True(UntypedEquals(x: Value1, y: Value1), userMessage: "value1 should equal itself");
    }

    [Fact]
    public static void UntypedEqualsZeroObjectDifferentToAnotherTypeOfObject()
    {
        object other = new();

        Assert.False(UntypedEquals(x: ZeroObject, y: other), userMessage: "zero object should not equal another type");
    }

    [Fact]
    public static void UntypedEqualsZeroObjectDifferentToNullObject()
    {
        Assert.False(UntypedEquals(x: ZeroObject, y: null), userMessage: "zero object should not equal null");
    }

    [Fact]
    public static void UntypedEqualsZeroObjectIsSameAsZeroObject()
    {
        Assert.True(UntypedEquals(x: ZeroObject, y: ZeroObject), userMessage: "zero object should equal itself");
    }
}
