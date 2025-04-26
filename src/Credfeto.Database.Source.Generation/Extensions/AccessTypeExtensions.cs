using System;
using Credfeto.Database.Source.Generation.Models;

namespace Credfeto.Database.Source.Generation.Extensions;

internal static class AccessTypeExtensions
{
    public static string ToKeywords(this AccessType accessType)
    {
        string accessTypeString = accessType switch
        {
            AccessType.PUBLIC => "public",
            AccessType.PRIVATE => "private",
            AccessType.PROTECTED => "protected",
            AccessType.PROTECTED_INTERNAL => "protected internal",
            AccessType.INTERNAL => "internal",
            _ => throw new ArgumentOutOfRangeException(nameof(accessType), actualValue: accessType, message: null),
        };

        return accessTypeString;
    }
}
