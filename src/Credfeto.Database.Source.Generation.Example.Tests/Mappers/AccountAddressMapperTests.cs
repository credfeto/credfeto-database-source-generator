using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Credfeto.Database.Source.Generation.Example.Mappers;
using Credfeto.Database.Source.Generation.Example.Primatives;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Database.Source.Generation.Example.Tests.Mappers;

public sealed class AccountAddressMapperTests : TestBase
{
    [Fact]
    public void MapFromDb_WithStringValue_ReturnsAccountAddressWithMatchingValue()
    {
        const string EXPECTED_VALUE = "0x1234567890abcdef";

        AccountAddress result = AccountAddressMapper.MapFromDb(EXPECTED_VALUE);

        Assert.Equal(EXPECTED_VALUE, result.Value, StringComparer.Ordinal);
    }

    [Fact]
    public void MapToDb_SetsValueDbTypeAndSize()
    {
        const string ADDRESS_VALUE = "0x1234567890abcdef";
        AccountAddress address = new() { Value = ADDRESS_VALUE };
        TestDbParameter parameter = new();

        AccountAddressMapper.MapToDb(value: address, parameter: parameter);

        Assert.Equal(ADDRESS_VALUE, parameter.Value);
        Assert.Equal(DbType.String, parameter.DbType);
        Assert.Equal(100, parameter.Size);
    }

    private sealed class TestDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; }

        public override bool IsNullable { get; set; }

        [AllowNull]
        public override string ParameterName { get; set; } = string.Empty;

        public override int Size { get; set; }

        [AllowNull]
        public override string SourceColumn { get; set; } = string.Empty;

        public override bool SourceColumnNullMapping { get; set; }

        public override object? Value { get; set; }

        public override void ResetDbType() { }
    }
}
