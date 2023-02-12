using System;
using Credfeto.Database.Source.Generation.Builders;
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

        CodeBuilder cb = new();

        foreach (MethodDeclarationSyntax method in receiver.Methods)
        {
            cb.AppendLine($"// {method.ReturnType} {method.Identifier.Text}({method.ParameterList});");
        }

        Console.WriteLine(receiver.GetType()
                                  .FullName);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }
}