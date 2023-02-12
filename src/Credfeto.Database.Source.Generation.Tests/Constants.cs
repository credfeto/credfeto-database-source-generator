namespace Credfeto.Database.Source.Generation.Tests;

internal static class Constants
{
    public const string DatabaseTypes = @"
namespace Credfeto.Database.Interfaces
{
    public enum SqlObjectType
    {
        SCALAR_FUNCTION,
        TABLE_FUNCTION,
        STORED_PROCEDURE
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SqlObjectMapAttribute : Attribute
    {
        public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType)
        {
            this.Name = name;
            this.SqlObjectType = sqlObjectType;
        }

        public string Name { get; }

        public SqlObjectType SqlObjectType { get; }
    }

    public interface IMapper<T>
    {
        [SuppressMessage(category: ""Design"", checkId: ""MA0018:Do not declare a static member on generic types"", Justification = ""By Design"")]
        [SuppressMessage(category: ""Design"", checkId: ""CA1000:Do not declare a static member on generic types"", Justification = ""By Design"")]
        abstract static T MapFromDb(object thing);

        [SuppressMessage(category: ""Design"", checkId: ""MA0018:Do not declare a static member on generic types"", Justification = ""By Design"")]
        [SuppressMessage(category: ""Design"", checkId: ""CA1000:Do not declare a static member on generic types"", Justification = ""By Design"")]
        abstract static void MapToDb(T thing, DbParameter parameter);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class SqlFieldMapAttribute<TM, TD> : Attribute
        where TM : IMapper<TD>
    {
    }
}
";

    public const string AccountAddressClass = @"
namespace Primatives
{
    [DebuggerDisplay(""{Value}"")]
    public sealed class AccountAddress
    {
        public required string Value { get; init; }
    }
}
";

    public const string AccountAddressMapperClass = @"
namespace Mappers
{
    [SuppressMessage(category: ""Microsoft.Performance"", checkId: ""CA1812:AvoidUninstantiatedInternalClasses"", Justification = ""Unit test"")]
    [SuppressMessage(category: ""ReSharper"", checkId: ""ClassNeverInstantiated.Local"", Justification = ""Unit test"")]
    internal sealed class AccountAddressMapper : IMapper<AccountAddress>
    {
        public static AccountAddress MapFromDb(object thing)
        {
            return new() { Value = (string)thing };
        }

        public static void MapToDb(AccountAddress thing, DbParameter parameter)
        {
            parameter.Value = thing.Value;
            parameter.DbType = DbType.String;
            parameter.Size = 100;
        }
    }
}
";

    public const string AccountModelClass = @"
namespace Models
{
    [DebuggerDisplay(""Id = {Id}, Name = {Name}, Address = {Address}"")]
    public sealed record Account(int Id,
                                 string Name,
                                 [SqlFieldMap<AccountAddressMapper, AccountAddress>]
                                 AccountAddress Address);
}
";
}