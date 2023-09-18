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
            DatabaseSourceCodeGenerator.ReportErrors(context: context, receiver: receiver);

            return;
        }

        DatabaseSourceCodeGenerator.GenerateMethods(context: context, receiver: receiver);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new DatabaseSyntaxReceiver());
    }
}