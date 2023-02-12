using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(string namespaceName, AccessType classAccessType, string className, AccessType methodAccessType, MethodDeclarationSyntax method)
    {
        this.NamespaceName = namespaceName;
        this.ClassAccessType = classAccessType;
        this.ClassName = className;
        this.MethodAccessType = methodAccessType;
        this.Method = method;
    }

    public string NamespaceName { get; }

    public AccessType ClassAccessType { get; }

    public string ClassName { get; }

    public AccessType MethodAccessType { get; }

    public MethodDeclarationSyntax Method { get; }
}