using Credfeto.Database.Source.Generation.Example.Models;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using FunFair.Test.Infrastructure.Mocks;

namespace Credfeto.Database.Source.Generation.Example.Tests.Models;

public sealed class AccountsTests : EquatableObjectTestBase<Accounts>
{
    private static readonly AccountAddress SharedAddress = new() { Value = "0x1234567890abcdef" };

    public AccountsTests()
        : base(
            zeroObject: new Accounts(
                Id: 0,
                Name: string.Empty,
                Address: SharedAddress,
                LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
            ),
            value1: new Accounts(
                Id: 1,
                Name: "Test Account",
                Address: SharedAddress,
                LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
            ),
            equivalentToValue1: new Accounts(
                Id: 1,
                Name: "Test Account",
                Address: SharedAddress,
                LastModified: MockDateTimeSources.Past.GetUtcNow().UtcDateTime
            )
        ) { }

    protected override bool OperatorEquals(Accounts? x, Accounts? y)
    {
        return x == y;
    }

    protected override bool OperatorNotEquals(Accounts? x, Accounts? y)
    {
        return x != y;
    }
}
