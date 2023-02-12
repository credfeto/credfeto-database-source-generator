using System.Collections.Generic;

namespace Credfeto.Database.Source.Generation.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TItemType> RemoveNulls<TItemType>(this IEnumerable<TItemType?> source)
        where TItemType : class
    {
        foreach (TItemType? item in source)
        {
            if (!ReferenceEquals(objA: item, objB: null))
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<TItemType> RemoveNulls<TItemType>(this IEnumerable<TItemType?> source)
        where TItemType : struct
    {
        foreach (TItemType? item in source)
        {
            if (item.HasValue)
            {
                yield return item.Value;
            }
        }
    }
}