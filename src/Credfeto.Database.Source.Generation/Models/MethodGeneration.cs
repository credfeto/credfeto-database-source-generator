using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(ClassInfo containingContext, MethodInfo methodInfo, SemanticModel semanticModel, SqlObject sqlObject)
    {
        this.ContainingContext = containingContext;
        this.Method = methodInfo;
        this.SemanticModel = semanticModel;
        this.SqlObject = sqlObject;
    }

    public ClassInfo ContainingContext { get; }

    public MethodInfo Method { get; }

    public SemanticModel SemanticModel { get; }

    public SqlObject SqlObject { get; }

    public string MethodGrouping => string.Join(separator: ".", this.ContainingContext.Namespace, this.ContainingContext.Name, this.Method.Name);
}