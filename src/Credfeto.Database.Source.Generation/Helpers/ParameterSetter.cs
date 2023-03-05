using System.Data;
using Credfeto.Database.Source.Generation.Builders;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Extensions;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class ParameterSetter
{
    public static void SetParamerterInfo(CodeBuilder source, string parameterObject, string parameterName, string typeName)
    {
        DbType dbType = TypeMapper.Map(typeName) ?? throw new InvalidModelException($"Cannot determine DbType for type {typeName} for parameter {parameterName}. Does it need a mapper?");
        source.AppendLine($"{parameterObject}.DbType = {nameof(DbType)}.{dbType.GetName()};");
    }
}