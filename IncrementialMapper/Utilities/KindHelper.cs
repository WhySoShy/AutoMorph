using System;
using IncrementialMapper.Syntax.Kinds;

namespace IncrementialMapper.Utilities;

internal static class KindHelper
{
    public static string ToReadAbleString(this VisibilityKind visibility)
        => visibility switch
        {
            VisibilityKind.Public => "public",
            VisibilityKind.Protected => "protected",
            VisibilityKind.Internal => "internal",
            VisibilityKind.Private => "private",
            _ => throw new NotSupportedException("This visibility is not supported.")
        };

    public static string ToReadableString(this ModifierKind modifier)
        => modifier switch
        {
            ModifierKind.Partial => "partial",
            ModifierKind.Static => "static",
            _ => throw new NotSupportedException("This modifier is not supported.")
        };
}