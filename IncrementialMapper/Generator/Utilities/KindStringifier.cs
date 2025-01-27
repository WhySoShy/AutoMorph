using System;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;

namespace IncrementialMapper.Generator.Utilities;

internal static class KindStringifier
{
    const string IENUMERABLE = "global::System.Collections.Generic.IEnumerable";
    const string IQUERYABLE = "global::System.Linq.IQueryable";
    
    internal static string ToReadAbleString(this VisibilityKind visibility)
        => visibility switch
        {
            VisibilityKind.Public => "public",
            VisibilityKind.Protected => "protected",
            VisibilityKind.Internal => "internal",
            VisibilityKind.Private => "private",
            _ => throw new NotSupportedException("This visibility is not supported.")
        };

    internal static string ToReadableString(this ModifierKind modifier)
        => modifier switch
        {
            ModifierKind.Partial => "partial",
            ModifierKind.Static => "static",
            _ => throw new NotSupportedException("This modifier is not supported.")
        };

    internal static string GetMethodKindAsString(this MethodToken token, ReferenceClassToken classToken)
        => token.Type switch
        {
            MethodKind.Standard => $"{classToken.FullPath}",
            MethodKind.Linq => $"{IENUMERABLE}<{classToken.FullPath}>",
            MethodKind.Linq_IQueryable => $"{IQUERYABLE}<{classToken.FullPath}>",
            _ => throw new NotSupportedException("This method is not supported.")
        };

    /// <summary>
    /// <para>
    ///     This is the reference used, to map from the source class to the target class.
    ///     This will always be either x or source, depending on the return type. <br />
    ///     For example;
    ///     <code>new ClassX() { Property = source.Property }</code>
    ///     <code>source.Select(x => new ClassX() { Property = x.Property })</code>
    /// </para>
    /// </summary>
    internal static string GetReferenceSourceName(this MethodToken token)
        => token.Type switch
        {
            MethodKind.Standard => "source",
            MethodKind.Linq_IQueryable or MethodKind.Linq => "x",
            _ => throw new NotSupportedException("This method is not supported.")
        };

    internal static string GetModifierAsString(this ModifierKind modifier)
        => modifier switch
        {
            ModifierKind.Partial => "partial",
            ModifierKind.Static => "static",
            ModifierKind.None => string.Empty,
            _ => throw new NotSupportedException("This modifier is not supported.")
        };
}