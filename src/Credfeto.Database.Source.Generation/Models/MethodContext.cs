using System.Collections.Generic;
using System.Diagnostics;

namespace Credfeto.Database.Source.Generation.Models;

[DebuggerDisplay("{MethodGeneration}")]
internal readonly struct MethodContext
{
    public MethodGeneration? MethodGeneration { get; }

    public List<WarningModelInfo>? Warnings { get; }

    public InvalidModelInfo? InvalidModel { get; }

    public ErrorInfo? ErrorInfo { get; }

    public MethodContext()
        : this(methodGeneration: null, warnings: null, invalidModel: null, errorInfo: null)
    {
    }

    public MethodContext(MethodGeneration? methodGeneration, List<WarningModelInfo>? warnings)
        : this(methodGeneration: methodGeneration, warnings: warnings, invalidModel: null, errorInfo: null)
    {
    }

    public MethodContext(InvalidModelInfo? invalidModel)
        : this(methodGeneration: null, warnings: null, invalidModel: invalidModel, errorInfo: null)
    {
    }

    public MethodContext(ErrorInfo? errorInfo)
        : this(methodGeneration: null, warnings: null, invalidModel: null, errorInfo: errorInfo)
    {
    }

    private MethodContext(MethodGeneration? methodGeneration, List<WarningModelInfo>? warnings, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo)
    {
        this.MethodGeneration = methodGeneration;
        this.Warnings = warnings;
        this.InvalidModel = invalidModel;
        this.ErrorInfo = errorInfo;
    }
}