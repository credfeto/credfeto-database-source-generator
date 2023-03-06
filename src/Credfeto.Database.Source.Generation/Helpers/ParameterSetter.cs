using System;
using System.Data;
using Credfeto.Database.Source.Generation.Builders;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Extensions;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class ParameterSetter
{
    public static void SetParamerterInfo(CodeBuilder source, string parameterObject, string parameterName, string typeName)
    {
        bool isNullable = typeName.EndsWith(value: "?", comparisonType: StringComparison.Ordinal);
        string nonNullableType = isNullable
            ? typeName.Substring(startIndex: 0, typeName.Length - 1)
            : typeName;

        DbType dbType = TypeMapper.Map(nonNullableType) ?? ThrowInvalidDbType(parameterName: parameterName, typeName: nonNullableType);

        if (isNullable)
        {
            AddNullableParameter(source: source, parameterObject: parameterObject, parameterName: parameterName, dbType: dbType);
        }
        else
        {
            AddNonNullableParameter(source: source, parameterObject: parameterObject, parameterName: parameterName, dbType: dbType);
        }
    }

    private static void AddNonNullableParameter(CodeBuilder source, string parameterObject, string parameterName, DbType dbType)
    {
        source.AppendLine($"{parameterObject}.DbType = {nameof(DbType)}.{dbType.GetName()};")
              .AppendLine($"{parameterObject}.Value = {parameterName};");

        SetParameterLength(source: source, parameterObject: parameterObject, parameterName: parameterName, dbType: dbType);
    }

    private static void AddNullableParameter(CodeBuilder source, string parameterObject, string parameterName, DbType dbType)
    {
        source.AppendLine($"{parameterObject}.DbType = {nameof(DbType)}.{dbType.GetName()};");

        using (source.StartBlock($"if({parameterName} == null)"))
        {
            source.AppendLine($"{parameterObject}.Value = DBNull.Value;");
        }

        using (source.StartBlock("else"))
        {
            source.AppendLine($"{parameterObject}.Value = {parameterName};");

            SetParameterLength(source: source, parameterObject: parameterObject, parameterName: parameterName, dbType: dbType);
        }
    }

    private static void SetParameterLength(CodeBuilder source, string parameterObject, string parameterName, DbType dbType)
    {
        if (dbType == DbType.String)
        {
            source.AppendLine($"{parameterObject}.Size = {parameterName}.Length;");
        }
        else if (dbType == DbType.Binary)
        {
            source.AppendLine($"{parameterObject}.Size = {parameterName}.Length;");
        }
    }

    private static DbType ThrowInvalidDbType(string parameterName, string typeName)
    {
        throw new InvalidModelException($"Cannot determine DbType for type {typeName} for parameter {parameterName}. Does it need a mapper?");
    }
}