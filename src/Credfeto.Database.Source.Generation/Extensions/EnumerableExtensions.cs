using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Extensions;

public static class EnumerableExtensions
{
    [SuppressMessage(
        category: "SonarAnalyzer.CSharp",
        checkId: "S3267:Loops should be simplified with LINQ",
        Justification = "For performance reasons"
    )]
    public static IEnumerable<TItemType> RemoveNulls<TItemType>(this IEnumerable<TItemType?> source)
        where TItemType : class
    {
        foreach (TItemType? item in source)
        {
            if (item is not null)
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
