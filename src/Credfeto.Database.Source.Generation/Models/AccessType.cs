using Microsoft.CodeAnalysis.CSharp;

namespace Credfeto.Database.Source.Generation.Models;

internal enum AccessType
{
    PUBLIC = SyntaxKind.PublicKeyword,
    PRIVATE = SyntaxKind.PrivateKeyword,
    PROTECTED = SyntaxKind.ProtectedKeyword,
    PROTECTED_INTERNAL = SyntaxKind.ProtectedKeyword | SyntaxKind.InternalKeyword,
    INTERNAL = SyntaxKind.InternalKeyword,
}
