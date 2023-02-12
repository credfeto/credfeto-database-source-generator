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
        string methodStaticModifier = method.IsStatic
            ? "static "
            : string.Empty;

        using (source.AppendLine($"[GeneratedCode(tool: \"{typeof(DatabaseCodeGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                     .StartBlock($"{method.MethodAccessType.ToKeywords()} {methodStaticModifier}async partial {method.Method.ReturnType} {method.Method.Identifier.Text}{method.Method.ParameterList}"))
        {
            source.AppendLine("await Task.CompletedTask;");
            source.AppendLine("throw new NotImplementedException();");
        }

        source.AppendLine("/*");
        source.AppendLine($" {method.ContainingContext.Namespace} {method.ContainingContext.AccessType.GetName()} {classStaticModifier} partial {method.ContainingContext.Name}");

        source.AppendLine("*/");
    }
}