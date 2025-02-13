namespace Credfeto.Database.Source.Generation.Models;

internal sealed class ClassInfo
{
    public ClassInfo(string namespaceName, string name, AccessType accessType, bool isStatic)
    {
        this.Namespace = namespaceName;
        this.Name = name;
        this.AccessType = accessType;
        this.IsStatic = isStatic;
    }

    public string Namespace { get; }

    public string Name { get; }

    public AccessType AccessType { get; }

    public bool IsStatic { get; }
}
