using Credfeto.Database.Source.Generation.Builders;
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

        CodeBuilder source = new();

        foreach (MethodDeclarationSyntax method in receiver.Methods)
        {
            source.AppendLine("/*");
            source.AppendLine($" {method.ReturnType} {method.Parent}::{method.Identifier.Text}{method.ParameterList};");
            source.AppendLine("*/");
        }

        context.AddSource(hintName: "Database.generated.cs", sourceText: source.Text);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }
}