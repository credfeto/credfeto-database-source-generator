using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodInfo
{
    public MethodInfo(AccessType accessType, bool isStatic, string name, string returnType, IReadOnlyList<string> attributes, MethodDeclarationSyntax method)
    {
        this.AccessType = accessType;
        this.IsStatic = isStatic;
        this.Name = name;
        this.ReturnType = returnType;
        this.Attributes = attributes;
        this.Method = method;
    }

    public AccessType AccessType { get; }

    public bool IsStatic { get; }

    public string Name { get; }

    public string ReturnType { get; }

    public IReadOnlyList<string> Attributes { get; }

    public MethodDeclarationSyntax Method { get; }
}