using System.Collections.Generic;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodInfo
{
    public MethodInfo(AccessType accessType, bool isStatic, string name, MethodReturnType returnType, IReadOnlyList<MethodParameter> parameters)
    {
        this.AccessType = accessType;
        this.IsStatic = isStatic;
        this.Name = name;
        this.ReturnType = returnType;
        this.Parameters = parameters;
    }

    public AccessType AccessType { get; }

    public bool IsStatic { get; }

    public string Name { get; }

    public MethodReturnType ReturnType { get; }

    public IReadOnlyList<MethodParameter> Parameters { get; }
}