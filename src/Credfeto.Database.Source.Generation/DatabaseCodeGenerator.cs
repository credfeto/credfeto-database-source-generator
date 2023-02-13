using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Credfeto.Database.Source.Generation.Builders;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Helpers;
using Credfeto.Database.Source.Generation.Models;
using Credfeto.Database.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                GenerateScalarFunctionMethod(method: method, source: source, classStaticModifier: classStaticModifier);

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

    private static void GenerateScalarFunctionMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (BuildFunctionSignature(source: source, method: method))
            {
                // TODO: Add Parameters, if any.
                source.AppendLine("DbCommand command = connection.CreateCommand();")
                      .AppendLine($"command.CommandText = \"select {method.SqlObject.Name}()\";")
                      .AppendBlankLine()
                      .AppendLine("object? result = await command.ExecuteScalarAsync(cancellationToken: cancellationToken);")
                      .AppendBlankLine();

                // TODO: Handle null/DBNull when type is nullable.
                using (source.StartBlock(text: "if (result is null)"))
                {
                    source.AppendLine("throw new InvalidOperationException(\"No result returned.\");");
                }

                // TODO - Handle types other than int
                using (source.StartBlock(text: "if (result is int value)"))
                {
                    source.AppendLine("return value;");
                }

                source.AppendLine("return Convert.ToInt32(value: result, provider: CultureInfo.InvariantCulture);");
            }
        }

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");
        }
    }

    private static IDisposable BuildFunctionSignature(CodeBuilder source, MethodGeneration method)
    {
        try
        {
            return BuildFunctionSignature(source: source, method: method.Method);
        }
        finally
        {
            DumpParameters(method: method, source: source);
        }
    }

    private static IDisposable BuildFunctionSignature(CodeBuilder source, MethodInfo method)
    {
        string methodStaticModifier = method.IsStatic
            ? "static "
            : string.Empty;

        return source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                     .StartBlock($"{method.AccessType.ToKeywords()} {methodStaticModifier}async partial {method.ReturnType.ReturnType.ToDisplayString()} {method.Name}{method.Method.ParameterList}");
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

    private static void DumpParameters(MethodGeneration method, CodeBuilder source)
    {
        SeparatedSyntaxList<ParameterSyntax> parameters = method.Method.Method.ParameterList.Parameters;

        foreach (ParameterSyntax parameter in parameters)
        {
            string parameterName = parameter.Identifier.Text;
            ISymbol? pType = method.SemanticModel.GetSymbol(parameter);

            MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: method.SemanticModel, parameterSyntax: parameter);

            if (pType is null)
            {
                source.AppendLine($"// TODO: Add parameter {parameterName} of type ????");
            }
            else
            {
                string displayType = pType.ToDisplayString();

                if (displayType == typeof(DbConnection).FullName || displayType == typeof(CancellationToken).FullName)
                {
                    source.AppendLine($"// TODO: Add C# parameter {parameterName} of type {pType.ToDisplayString()}");
                }
                else
                {
                    source.AppendLine($"// TODO: Add DB parameter {parameterName} of type {pType.ToDisplayString()}");

                    if (mapperInfo != null)
                    {
                        source.AppendLine($" -> Using {mapperInfo.MapperSymbol.ToDisplayString()} to map to {mapperInfo.MappedSymbol.ToDisplayString()}");
                    }
                }
            }
        }
    }
}