using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Database.Source.Generation.Builders;
using Credfeto.Database.Source.Generation.Extensions;
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

        string staticModifier = firstMethod.ContainingContext.IsStatic
            ? "static "
            : string.Empty;

        using (source.AppendLine($"namespace {firstMethod.ContainingContext.Namespace};")
                     .AppendBlankLine()
                     .StartBlock($"{firstMethod.ContainingContext.AccessType.ToKeywords()} {staticModifier}partial class {firstMethod.ContainingContext.Name}"))
        {
            foreach (MethodGeneration? method in methods)
            {
                source.AppendLine("/*");
                source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {staticModifier} partial {method.ContainingContext.Name}");
                source.AppendLine($" {method.MethodAccessType.GetName()} {method.Method.ReturnType} {method.Method.Identifier.Text}{method.Method.ParameterList};");
                source.AppendLine("*/");
            }
        }

        context.AddSource($"{fullName}.Database.generated.cs", sourceText: source.Text);
    }
}