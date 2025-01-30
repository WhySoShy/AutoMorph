using AutoMorph.Internal.Generator.Validators;

namespace AutoMorph.Internal.Syntax.Kinds;


public enum PropertyKind
{
    None = 0,
    /// <summary>
    /// See supported collections in <see cref="ValidCollections.SupportedCollections"/>
    /// </summary>
    Collection = 1,
    Object = 2,
}