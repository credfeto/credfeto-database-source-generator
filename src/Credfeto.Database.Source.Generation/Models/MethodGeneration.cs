namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(ClassInfo containingContext, MethodInfo methodInfo)
    {
        this.ContainingContext = containingContext;
        this.Method = methodInfo;
    }

    public ClassInfo ContainingContext { get; }

    public MethodInfo Method { get; }

    public string MethodGrouping => $"{this.ContainingContext.Namespace}.{this.ContainingContext.Name}.{this.Method.Method.Identifier.Text}";
}