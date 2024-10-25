using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodGeneration
{
    public MethodGeneration(ClassInfo containingContext, MethodToGenerate methodToGenerate, SemanticModel semanticModel, SqlObject sqlObject, Location location)
    {
        this.ContainingContext = containingContext;
        this.Method = methodToGenerate;
        this.SemanticModel = semanticModel;
        this.SqlObject = sqlObject;
        this.Location = location;
    }

    public ClassInfo ContainingContext { get; }

    public MethodToGenerate Method { get; }

    public SemanticModel SemanticModel { get; }

    public SqlObject SqlObject { get; }

    public Location Location { get; }

    public string FullName => string.Join(separator: ".", this.ContainingContext.Namespace, this.ContainingContext.Name, this.Method.Name);
}