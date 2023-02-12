using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(ClassInfo containingContext, AccessType methodAccessType, MethodDeclarationSyntax method)
    {
        this.ContainingContext = containingContext;
        this.MethodAccessType = methodAccessType;
        this.Method = method;
    }

    public ClassInfo ContainingContext { get; }

    public AccessType MethodAccessType { get; }

    public MethodDeclarationSyntax Method { get; }

    public string MethodGrouping => $"{this.ContainingContext.Namespace}.{this.ContainingContext.Name}.{this.Method.Identifier.Text}";
}