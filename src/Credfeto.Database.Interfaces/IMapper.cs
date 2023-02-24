using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Interfaces;

public interface IMapper<T>
{
    [SuppressMessage(category: "Design", checkId: "MA0018:Do not declare a static member on generic types", Justification = "By Design")]
    [SuppressMessage(category: "Design", checkId: "CA1000:Do not declare a static member on generic types", Justification = "By Design")]
    abstract static T MapFromDb(object value);

    [SuppressMessage(category: "Design", checkId: "MA0018:Do not declare a static member on generic types", Justification = "By Design")]
    [SuppressMessage(category: "Design", checkId: "CA1000:Do not declare a static member on generic types", Justification = "By Design")]
    abstract static void MapToDb(T value, DbParameter parameter);
}