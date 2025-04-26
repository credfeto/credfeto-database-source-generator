using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Interfaces;

public interface IMapper<T>
{
    [SuppressMessage(
        category: "Design",
        checkId: "MA0018:Do not declare a static member on generic types",
        Justification = "By Design"
    )]
    [SuppressMessage(
        category: "Design",
        checkId: "CA1000:Do not declare a static member on generic types",
        Justification = "By Design"
    )]
    [SuppressMessage(
        category: "Microsoft.IDE",
        checkId: "IDE0036: Modifiers are not ordered",
        Justification = "CSharpifier requires"
    )]
    [SuppressMessage(category: "ReSharper", checkId: "ArrangeModifiersOrder", Justification = "CSharpifier requires")]
    static abstract T MapFromDb(object value);

    [SuppressMessage(
        category: "Design",
        checkId: "MA0018:Do not declare a static member on generic types",
        Justification = "By Design"
    )]
    [SuppressMessage(
        category: "Design",
        checkId: "CA1000:Do not declare a static member on generic types",
        Justification = "By Design"
    )]
    [SuppressMessage(
        category: "Microsoft.IDE",
        checkId: "IDE0036: Modifiers are not ordered",
        Justification = "By Design"
    )]
    [SuppressMessage(category: "ReSharper", checkId: "ArrangeModifiersOrder", Justification = "CSharpifier requires")]
    static abstract void MapToDb(T value, DbParameter parameter);
}
