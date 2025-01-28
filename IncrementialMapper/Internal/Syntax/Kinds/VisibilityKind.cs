using System;

namespace IncrementialMapper.Internal.Syntax.Kinds;

internal enum VisibilityKind
{
    Public,
    Private,
    Protected,
    Internal,
    
    // Don't know if these are being supported.
    // They could technically be supported for partial classes.
    Protected_Private,
    Protected_Internal
}

internal static class VisibilityKindExtensions
{
    internal static string ToReadAbleString(this VisibilityKind visibility)
        => visibility switch
        {
            VisibilityKind.Public => "public",
            VisibilityKind.Protected => "protected",
            VisibilityKind.Internal => "internal",
            VisibilityKind.Private => "private",
            _ => throw new NotSupportedException("This visibility is not supported.")
        };
}