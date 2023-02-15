using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(ClassInfo containingContext, MethodToGenerate methodToGenerate, SemanticModel semanticModel, SqlObject sqlObject)
    {
        this.ContainingContext = containingContext;
        this.Method = methodToGenerate;
        this.SemanticModel = semanticModel;
        this.SqlObject = sqlObject;
    }

    public ClassInfo ContainingContext { get; }

    public MethodToGenerate Method { get; }

    public SemanticModel SemanticModel { get; }

    public SqlObject SqlObject { get; }

    public string MethodGrouping => string.Join(separator: ".", this.ContainingContext.Namespace, this.ContainingContext.Name, this.Method.Name);
}