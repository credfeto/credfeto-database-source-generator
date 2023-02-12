using System;
using System.Collections.Generic;
using System.Linq;
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
        MethodGeneration firstMethod = methods.First();

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

    private static void GenerateScalarFunctionMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        string methodStaticModifier = method.Method.IsStatic
            ? "static "
            : string.Empty;

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                         .StartBlock(
                             $"{method.Method.AccessType.ToKeywords()} {methodStaticModifier}async partial {method.Method.Method.ReturnType} {method.Method.Method.Identifier.Text}{method.Method.Method.ParameterList}"))
            {
                source.AppendLine($"-- {method.SqlObject.Name} {method.SqlObject.SqlObjectType.GetName()}");

                // foreach (string attr in method.Method.Attributes)
                // {
                //     source.AppendLine($"Attribute: {attr}");
                // }
                //
                // AttributeSyntax[] stuff = method.Method.Method.AttributeLists.SelectMany(selector: x => x.Attributes).ToArray();
                //
                // foreach (var thing in stuff)
                // {
                //     ISymbol? symbol = method.SemanticModel.GetDeclaredSymbol(declaration: thing);
                //
                //     if (symbol != null)
                //     {
                //         source.AppendLine($"Symbol: {symbol.ToDisplayString()}");
                //     }
                // }

                source.AppendLine($"// {method.Method.ReturnType}");
                source.AppendLine("await Task.CompletedTask;");
                source.AppendLine("throw new NotImplementedException();");
            }
        }

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");
        }
    }

    private static void GenerateTableFunctionMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        string methodStaticModifier = method.Method.IsStatic
            ? "static "
            : string.Empty;

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                         .StartBlock(
                             $"{method.Method.AccessType.ToKeywords()} {methodStaticModifier}async partial {method.Method.Method.ReturnType} {method.Method.Method.Identifier.Text}{method.Method.Method.ParameterList}"))
            {
                source.AppendLine("await Task.CompletedTask;");
                source.AppendLine("throw new NotImplementedException();");
            }
        }

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");
        }
    }

    private static void GenerateStoredProcedureMethod(MethodGeneration method, CodeBuilder source, string classStaticModifier)
    {
        string methodStaticModifier = method.Method.IsStatic
            ? "static "
            : string.Empty;

        using (source.StartBlock(text: "", start: "/*", end: "*/"))
        {
            using (source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                         .StartBlock(
                             $"{method.Method.AccessType.ToKeywords()} {methodStaticModifier}async partial {method.Method.Method.ReturnType} {method.Method.Method.Identifier.Text}{method.Method.Method.ParameterList}"))
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
}