using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Stuff;

public interface IMapper<T>
{
    [SuppressMessage(category: "Design", checkId: "MA0018:Do not declare a static member on generic types", Justification = "By Design")]
    [SuppressMessage(category: "Design", checkId: "CA1000:Do not declare a static member on generic types", Justification = "By Design")]
    abstract static T MapFromDb(object thing);

    [SuppressMessage(category: "Design", checkId: "MA0018:Do not declare a static member on generic types", Justification = "By Design")]
    [SuppressMessage(category: "Design", checkId: "CA1000:Do not declare a static member on generic types", Justification = "By Design")]
    abstract static void MapToDb(T thing, DbParameter parameter);
}