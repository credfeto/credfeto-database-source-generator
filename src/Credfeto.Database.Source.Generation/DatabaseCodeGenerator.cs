using System;
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

        Console.WriteLine(receiver.GetType()
                                  .FullName);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }
}