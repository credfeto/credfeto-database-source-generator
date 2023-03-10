using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Credfeto.Database.Source.Generation.Builders;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Helpers;
using Credfeto.Database.Source.Generation.Models;
using Credfeto.Database.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation;

[Generator]
public sealed class DatabaseCodeGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not DatabaseSyntaxReceiver receiver)
        {
            return;
        }

        if (receiver.Errors.Count != 0)
        {
            ReportErrors(context: context, receiver: receiver);

            return;
        }

        GenerateMethods(context: context, receiver: receiver);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }

    private static void ReportErrors(in GeneratorExecutionContext context, DatabaseSyntaxReceiver receiver)
    {
        foreach (InvalidModelInfo invalidModel in receiver.Errors)
        {
            context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "CDSG001",
                                                                       title: "Invalid model",
                                                                       messageFormat: invalidModel.Message,
                                                                       category: "Credfeto.Database.Source.Generation",
                                                                       defaultSeverity: DiagnosticSeverity.Error,
                                                                       isEnabledByDefault: true),
                                                                   location: invalidModel.Location));
        }
    }

    private static void GenerateMethods(in GeneratorExecutionContext context, DatabaseSyntaxReceiver receiver)
    {
        foreach (IGrouping<string, MethodGeneration> methodGroup in receiver.Methods.GroupBy(keySelector: m => m.MethodGrouping, comparer: StringComparer.OrdinalIgnoreCase))
        {
            IReadOnlyList<MethodGeneration> methods = methodGroup.ToArray();

            try
            {
                string fullName = methodGroup.Key;
                GenerateOneMethodGroup(context: context, methods: methods, fullName: fullName);
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "CDSG002",
                                                                           title: "Unhandled exception",
                                                                           messageFormat: exception.Message,
                                                                           category: "Credfeto.Database.Source.Generation",
                                                                           defaultSeverity: DiagnosticSeverity.Error,
                                                                           isEnabledByDefault: true),
                                                                       location: methodGroup.First()
                                                                                            .Location));
            }
        }
    }

    private static void GenerateOneMethodGroup(in GeneratorExecutionContext context, IReadOnlyList<MethodGeneration> methods, string fullName)
    {
        MethodGeneration firstMethod = methods[0];

        CodeBuilder source = new();

        string classStaticModifier = firstMethod.ContainingContext.IsStatic
            ? "static "
            : string.Empty;

        using (source.AppendLine("using System;")
                     .AppendLine("using System.CodeDom.Compiler;")
                     .AppendLine("using System.Collections.Generic;")
                     .AppendLine("using System.Data;")
                     .AppendLine("using System.Data.Common;")
                     .AppendLine("using System.Globalization;")
                     .AppendLine("using System.Linq;")
                     .AppendLine("using System.Threading;")
                     .AppendLine("using System.Threading.Tasks;")
                     .AppendBlankLine()
                     .AppendLine("#nullable enable")
                     .AppendBlankLine()
                     .AppendLine($"namespace {firstMethod.ContainingContext.Namespace};")
                     .AppendBlankLine()
                     .StartBlock($"{firstMethod.ContainingContext.AccessType.ToKeywords()} {classStaticModifier}partial class {firstMethod.ContainingContext.Name}"))
        {
            bool isFirst = true;

            foreach (MethodGeneration method in methods)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    source.AppendBlankLine();
                }

                GenerateMethod(method: method, source: source);
            }
        }

        context.AddSource($"{fullName}.Database.generated.cs", sourceText: source.Text);
    }

    private static void GenerateMethod(MethodGeneration method, CodeBuilder source)
    {
        switch (method.SqlObject.SqlObjectType)
        {
            case SqlObjectType.SCALAR_FUNCTION:
                GenerateScalarFunctionMethod(method: method, source: source);

                break;
            case SqlObjectType.TABLE_FUNCTION:
                GenerateTableFunctionMethod(method: method, source: source);

                break;
            case SqlObjectType.STORED_PROCEDURE:
                GenerateStoredProcedureMethod(method: method, source: source);

                break;
            default: throw new ArgumentOutOfRangeException(nameof(method), actualValue: method.SqlObject.SqlObjectType, message: "Unsupported SQL object type");
        }
    }

    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
    private static void GenerateTableFunctionMethod(MethodGeneration method, CodeBuilder source)
    {
        bool isCollection = method.Method.ReturnType.CollectionReturnType != null;

        using (BuildFunctionSignature(source: source, method: method))
        {
            string functionParameters = BuildFunctionParameters(method);
            ImmutableArray<IParameterSymbol> columns = ExtractColumnsFromConstructor((INamedTypeSymbol)method.Method.ReturnType.ElementReturnType!);
            string columnSelector = BuildFunctionColumns(columns: columns);

            string returnType = method.Method.ReturnType.ElementReturnType!.ToDisplayString();

            BuildExtractLocalMethod(source: source, returnType: returnType, columns: columns);

            source.AppendBlankLine()
                  .AppendLine("DbCommand command = connection.CreateCommand();")
                  .AppendLine($"command.CommandText = \"select {columnSelector} from {method.SqlObject.Name}({functionParameters})\";");
            AppendCommandParameters(source: source, method: method, command: "command");

            string commandBehaviour = isCollection
                ? nameof(CommandBehavior.Default)
                : nameof(CommandBehavior.SingleRow);

            using (source.AppendBlankLine()
                         .StartBlock($"using (IDataReader reader = await command.ExecuteReaderAsync(behavior: CommandBehavior.{commandBehaviour}, cancellationToken: cancellationToken))"))
            {
                source.AppendLine(isCollection
                                      ? "return Extract(reader: reader).ToArray();"
                                      : "return Extract(reader: reader).FirstOrDefault();");
            }
        }
    }

    private static void BuildExtractLocalMethod(CodeBuilder source, string returnType, in ImmutableArray<IParameterSymbol> columns)
    {
        Dictionary<string, string> generated = new(StringComparer.Ordinal);

        foreach (IParameterSymbol column in columns)
        {
            MapperInfo? mapperInfo = column.GetMapperInfo();

            if (mapperInfo != null)
            {
                continue;
            }

            string typeName = column.Type.ToDisplayString();

            if (generated.TryGetValue(key: typeName, value: out _))
            {
                continue;
            }

            string methodName = ExtractColumns.GenerateExtractColumnMapper(source: source, typeName: typeName) ??
                                throw new InvalidModelException($"Unsupported C# data type {typeName} for column {column.Name}, does it need a mapper?");

            generated.Add(key: typeName, value: methodName);
            source.AppendBlankLine();
        }

        using (source.StartBlock($"static IEnumerable<{returnType}> Extract(IDataReader reader)"))
        {
            foreach (string column in columns.Select(selector: column => column.Name))
            {
                source.AppendLine($"int ordinal{column} = reader.GetOrdinal(name: nameof({returnType}.{column}));");
            }

            using (source.StartBlock("while (reader.Read())"))
            {
                source.AppendLine($"yield return new {returnType}(");

                for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                {
                    bool isLast = columnIndex == columns.Length - 1;
                    string end = isLast
                        ? ");"
                        : ",";

                    IParameterSymbol column = columns[columnIndex];

                    AppendConstructorParameter(source: source, column: column, end: end, generated: generated);
                }
            }
        }
    }

    private static void AppendConstructorParameter(CodeBuilder source, IParameterSymbol column, string end, Dictionary<string, string> generated)
    {
        MapperInfo? mapperInfo = column.GetMapperInfo();

        if (mapperInfo != null)
        {
            source.AppendLine($"                         {column.Name}: {mapperInfo.MapperSymbol.ToDisplayString()}.MapFromDb(reader.GetValue(ordinal{column.Name})){end}");
        }
        else
        {
            string typeName = column.Type.ToDisplayString();

            if (!generated.TryGetValue(key: typeName, out string? mapper))
            {
                throw new InvalidModelException($"Unsupported C# data type {typeName} for column {column.Name}, does it need a mapper?");
            }

            source.AppendLine($"                         {column.Name}: {mapper}(reader.GetValue(ordinal{column.Name}), @\"{column.Name}\"){end}");
        }
    }

    private static string BuildFunctionColumns(in ImmutableArray<IParameterSymbol> columns)
    {
        return string.Join(separator: ", ", columns.Select(selector: p => p.Name));
    }

    private static ImmutableArray<IParameterSymbol> ExtractColumnsFromConstructor(INamedTypeSymbol returnType)
    {
        bool IsSameType(IMethodSymbol constructor)
        {
            if (constructor.IsStatic)
            {
                return false;
            }

            if (constructor.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            return constructor.Parameters.Length == 1 && constructor.Parameters[0]
                                                                    .Type.ToDisplayString() == returnType.ToDisplayString();
        }

        ImmutableArray<IParameterSymbol> columns = returnType.Constructors.Where(c => c.Parameters.Length > 0 && !IsSameType(c))
                                                             .Select(selector: c => c.Parameters)
                                                             .First();

        return columns;
    }

    private static void GenerateScalarFunctionMethod(MethodGeneration method, CodeBuilder source)
    {
        using (BuildFunctionSignature(source: source, method: method))
        {
            string functionParameters = BuildFunctionParameters(method);

            source.AppendLine("DbCommand command = connection.CreateCommand();")
                  .AppendLine($"command.CommandText = \"select {method.SqlObject.Name}({functionParameters})\";");
            AppendCommandParameters(source: source, method: method, command: "command");

            source.AppendBlankLine()
                  .AppendLine("object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);")
                  .AppendBlankLine();

            if (method.Method.ReturnType.ElementReturnType is null)
            {
                throw new InvalidOperationException("Return type is null");
            }

            // TODO: Handle null/DBNull when type is nullable.
            using (source.StartBlock(text: "if (result is null)"))
            {
                source.AppendLine(method.Method.ReturnType.IsNullable
                                      ? "return null;"
                                      : "throw new InvalidOperationException(\"No result returned.\");");
            }

            source.AppendBlankLine();

            if (method.Method.ReturnType.MapperInfo != null)
            {
                source.AppendLine($"return {method.Method.ReturnType.MapperInfo.MapperSymbol.ToDisplayString()}.MapFromDb(value: result);");
            }
            else
            {
                string typeName = method.Method.ReturnType.ElementReturnType.ToDisplayString();
                string returnString = ExtractColumns.GenerateReturn(typeName: typeName, variable: "result") ??
                                      throw new InvalidModelException($"Unsupported C# data type {typeName} for return type, does it need a mapper?");

                source.AppendLine(returnString);
            }
        }
    }

    private static void AppendCommandParameters(CodeBuilder source, MethodGeneration method, string command)
    {
        int parameterIndex = 0;

        foreach (MethodParameter parameter in method.Method.Parameters)
        {
            if (parameter.Usage == MethodParameterUsage.DB)
            {
                CreateParameter(source: source, command: command, parameterIndex: parameterIndex, parameter: parameter);

                ++parameterIndex;
            }
        }
    }

    private static void CreateParameter(CodeBuilder source, string command, int parameterIndex, MethodParameter parameter)
    {
        source.AppendLine($"DbParameter p{parameterIndex} = command.CreateParameter();");

        if (parameter.MapperInfo != null)
        {
            source.AppendLine($"{parameter.MapperInfo.MapperSymbol.ToDisplayString()}.MapToDb({parameter.Name}, p{parameterIndex});");
        }
        else
        {
            if (parameter.Type is IParameterSymbol ps)
            {
                ParameterSetter.SetParamerterInfo(source: source, $"p{parameterIndex}", parameterName: parameter.Name, ps.Type.ToDisplayString());
            }
            else
            {
                ParameterSetter.SetParamerterInfo(source: source, $"p{parameterIndex}", parameterName: parameter.Name, parameter.Type.ToDisplayString());
            }
        }

        source.AppendLine($"p{parameterIndex}.ParameterName = \"@{parameter.Name}\";")
              .AppendLine($"{command}.Parameters.Add(p{parameterIndex});");
    }

    private static string BuildFunctionParameters(MethodGeneration method)
    {
        static IEnumerable<string> Build(IReadOnlyList<MethodParameter> parameters)
        {
            foreach (MethodParameter parameter in parameters)
            {
                if (parameter.Usage == MethodParameterUsage.DB)
                {
                    yield return $"@{parameter.Name}";
                }
            }
        }

        return string.Join(separator: ", ", Build(parameters: method.Method.Parameters));
    }

    private static IDisposable BuildFunctionSignature(CodeBuilder source, MethodGeneration method)
    {
        return BuildFunctionSignature(source: source, method: method.Method);
    }

    [SuppressMessage(category: "", checkId: "ENUM001", Justification = "Temp code")]
    private static IDisposable BuildFunctionSignature(CodeBuilder source, MethodToGenerate method)
    {
        string methodStaticModifier = method.IsStatic
            ? "static "
            : string.Empty;

        source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]");
        StringBuilder stringBuilder = new($"{method.AccessType.ToKeywords()} {methodStaticModifier}async partial {method.ReturnType.ReturnType.ToDisplayString()} {method.Name}(");

        bool first = true;

        foreach (MethodParameter parameter in method.Parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                stringBuilder.Append(", ");
            }

            if (parameter.Type is IParameterSymbol ps)
            {
                stringBuilder.Append(ps.Type.ToDisplayString())
                             .Append(' ')
                             .Append(parameter.Name);
            }
            else
            {
                stringBuilder.Append(parameter.Type.ToDisplayString());
            }
        }

        stringBuilder.Append(')');

        return source.StartBlock(stringBuilder.ToString());
    }

    private static void GenerateStoredProcedureMethod(MethodGeneration method, CodeBuilder source)
    {
        using (BuildFunctionSignature(source: source, method: method))
        {
            if (method.Method.ReturnType.ElementReturnType != null)
            {
                ImmutableArray<IParameterSymbol> columns = ExtractColumnsFromConstructor((INamedTypeSymbol)method.Method.ReturnType.ElementReturnType!);

                string returnType = method.Method.ReturnType.ElementReturnType!.ToDisplayString();

                BuildExtractLocalMethod(source: source, returnType: returnType, columns: columns);
            }

            string functionParameters = BuildFunctionParameters(method);
            source.AppendLine("DbCommand command = connection.CreateCommand();")
                  .AppendLine($"command.CommandText = \"CALL {method.SqlObject.Name}({functionParameters})\";");
            AppendCommandParameters(source: source, method: method, command: "command");

            if (method.Method.ReturnType.ElementReturnType is null)
            {
                source.AppendLine("await command.ExecuteNonQueryAsync(cancellationToken);");
            }
            else
            {
                bool isCollection = method.Method.ReturnType.CollectionReturnType != null;
                string commandBehaviour = isCollection
                    ? nameof(CommandBehavior.Default)
                    : nameof(CommandBehavior.SingleRow);

                using (source.AppendBlankLine()
                             .StartBlock($"using (IDataReader reader = await command.ExecuteReaderAsync(behavior: CommandBehavior.{commandBehaviour}, cancellationToken: cancellationToken))"))
                {
                    source.AppendLine(isCollection
                                          ? "return Extract(reader: reader).ToArray();"
                                          : "return Extract(reader: reader).FirstOrDefault();");
                }
            }
        }
    }
}