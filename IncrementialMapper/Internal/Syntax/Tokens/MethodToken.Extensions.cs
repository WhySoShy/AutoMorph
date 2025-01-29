using System;
using IncrementialMapper.Internal.Syntax.Types;

namespace IncrementialMapper.Internal.Syntax.Tokens;

internal sealed partial record MethodToken
{
    const string IENUMERABLE = "global::System.Collections.Generic.IEnumerable";
    const string IQUERYABLE = "global::System.Linq.IQueryable";
    
    internal string GetMethodKindAsString(ReferenceClassToken classToken)
        => Type switch
        {
            MethodType.Standard => $"{classToken.FullPath}",
            MethodType.Linq => $"{IENUMERABLE}<{classToken.FullPath}>",
            MethodType.IQueryable => $"{IQUERYABLE}<{classToken.FullPath}>",
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
    internal string GetReferenceSourceName()
        => Type switch
        {
            MethodType.Standard => "source",
            MethodType.IQueryable or MethodType.Linq => "x",
            _ => throw new NotSupportedException("This method is not supported.")
        };
}