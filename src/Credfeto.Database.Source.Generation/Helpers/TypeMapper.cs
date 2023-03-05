using System.Data;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class TypeMapper
{
    public static DbType? Map(string typeName)
    {
        return typeName switch
        {
            "int" => DbType.Int32,
            "long" => DbType.Int64,
            "short" => DbType.Int16,
            "byte" => DbType.Byte,
            "decimal" => DbType.Decimal,
            "double" => DbType.Double,
            "float" => DbType.Single,
            "uint" => DbType.UInt32,
            "ulong" => DbType.UInt64,
            "ushort" => DbType.UInt16,
            "sbyte" => DbType.Byte,
            "char" => DbType.String,
            "bool" => DbType.Boolean,
            "string" => DbType.String,
            "DateTime" => DbType.DateTime,
            "DateTimeOffset" => DbType.DateTimeOffset,
            "TimeSpan" => DbType.Time,
            "Guid" => DbType.Guid,
            "byte[]" => DbType.Binary,
            _ => null
        };
    }
}