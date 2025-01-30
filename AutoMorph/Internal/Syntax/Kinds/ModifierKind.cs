using System;

namespace AutoMorph.Internal.Syntax.Kinds;

/// <summary>
/// Determines if the object should be created as static or partial.
/// </summary>
internal enum ModifierKind
{
    Static = 0,
    Partial = 1,
    None = 3,
}

internal static class ModifierKindExtensions
{
    internal static string GetModifierAsString(this ModifierKind modifier)
        => modifier switch
        {
            ModifierKind.Partial => "partial",
            ModifierKind.Static => "static",
            ModifierKind.None => string.Empty,
            _ => throw new NotSupportedException("This modifier is not supported.")
        };
}