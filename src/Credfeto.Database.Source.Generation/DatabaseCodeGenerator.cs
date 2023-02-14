using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Credfeto.Database.Source.Generation.Builders;
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

        foreach (IGrouping<string, MethodGeneration> methodGroup in receiver.Methods.GroupBy(keySelector: m => m.MethodGrouping, comparer: StringComparer.OrdinalIgnoreCase))
        {
            IReadOnlyList<MethodGeneration> methods = methodGroup.ToArray();
            string fullName = methodGroup.Key;
            GenerateOneMethodGroup(context: context, methods: methods, fullName: fullName);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }

    private static void GenerateOneMethodGroup(in GeneratorExecutionContext context, IReadOnlyList<MethodGeneration> methods, string fullName)
    {
        MethodGeneration firstMethod = methods[0];

        CodeBuilder source = new();

        string classStaticModifier = firstMethod.ContainingContext.IsStatic
            ? "static "
            : string.Empty;

        // TODO: add in any other using declarations that are needed
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

                GenerateMethod(method: method, source: source, classStaticModifier: classStaticModifier);
            }
        }

        context.AddSource($"{fullName}.Database.generated.cs", sourceText: source.Text);
    }

    private static void GenerateMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        switch (method.SqlObject.SqlObjectType)
        {
            case SqlObjectType.SCALAR_FUNCTION:
                GenerateScalarFunctionMethod(method: method, source: source);

                break;
            case SqlObjectType.TABLE_FUNCTION:
                GenerateTableFunctionMethod(method: method, source: source, classStaticModifier: classStaticModifier);

                break;
            case SqlObjectType.STORED_PROCEDURE:
                GenerateStoredProcedureMethod(method: method, source: source, classStaticModifier: classStaticModifier);

                break;
            default: throw new ArgumentOutOfRangeException(nameof(method), actualValue: method.SqlObject.SqlObjectType, message: "Unsupported SQL object type");
        }
    }

    private static void GenerateTableFunctionMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (BuildFunctionSignature(source: source, method: method))
            {
                string functionParameters = BuildFunctionParameters(method);

                source.AppendLine($"select * from {method.SqlObject.Name}({functionParameters})");

                source.AppendLine("await Task.CompletedTask;");
                source.AppendLine("throw new NotImplementedException();");
            }
        }

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");
        }
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
                source.AppendLine("throw new InvalidOperationException(\"No result returned.\");");
            }

            source.AppendBlankLine();

            if (method.Method.ReturnType.MapperInfo != null)
            {
                source.AppendLine($"return {method.Method.ReturnType.MapperInfo.MapperSymbol.ToDisplayString()}.MapFromDb(value: result);");
            }
            else
            {
                source.AppendLine($"return ({method.Method.ReturnType.ElementReturnType!.ToDisplayString()})result;");
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
                source.AppendLine($"DbParameter p{parameterIndex} = command.CreateParameter();");

                if (parameter.MapperInfo != null)
                {
                    source.AppendLine($"{parameter.MapperInfo.MapperSymbol.ToDisplayString()}.MapToDb({parameter.Name}, p{parameterIndex});");
                }
                else
                {
                    source.AppendLine("p{parameterIndex}.Value = {parameter.Name};");
                }

                source.AppendLine($"p{parameterIndex}.ParameterName = `\"@{parameter.Name}\";")
                      .AppendLine($"{command}.Parameters.Add(p{parameterIndex});");

                ++parameterIndex;
            }
        }
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

    private static IDisposable BuildFunctionSignature(CodeBuilder source, MethodInfo method)
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

            stringBuilder.Append(parameter.Type.ToDisplayString())
                         .Append(' ')
                         .Append(parameter.Name);
        }

        stringBuilder.Append(')');

        return source.StartBlock(stringBuilder.ToString());
    }

    private static void GenerateStoredProcedureMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (BuildFunctionSignature(source: source, method: method))
            {
                // TODO: Add Parameters, if any.
                source.AppendLine("DbCommand command = connection.CreateCommand();")
                      .AppendLine($"command.CommandText = \"{method.SqlObject.Name}\";")
                      .AppendLine("command.CommandType = CommandType.StoredProcedure;");
                AppendCommandParameters(source: source, method: method, command: "command");

                source.AppendLine($"-- {method.SqlObject.Name} {method.SqlObject.SqlObjectType.GetName()}");

                source.AppendLine("await Task.CompletedTask;");
                source.AppendLine("throw new NotImplementedException();");
            }
        }

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");
        }
    }
}