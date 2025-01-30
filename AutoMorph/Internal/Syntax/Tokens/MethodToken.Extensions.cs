using System;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Types;

namespace AutoMorph.Internal.Syntax.Tokens;

internal sealed partial record MethodToken
{
    const string IENUMERABLE = "global::System.Collections.Generic.IEnumerable";
    const string IQUERYABLE = "global::System.Linq.IQueryable";
    
    internal string GetMethodKindAsString(ReferenceClassToken classToken, bool isGeneric)
        => Type switch
        {
            MethodType.Standard => isGeneric ? AssemblyConstants.GENERIC_TYPE_NAME : classToken.FullPath,
            MethodType.Linq => $"{IENUMERABLE}<" + (isGeneric ? AssemblyConstants.GENERIC_TYPE_NAME : classToken.FullPath) + ">",
            MethodType.IQueryable => $"{IQUERYABLE}<" + (isGeneric ? AssemblyConstants.GENERIC_TYPE_NAME : classToken.FullPath) + ">",
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