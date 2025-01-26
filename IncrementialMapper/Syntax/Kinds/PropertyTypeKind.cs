using IncrementialMapper.Utilities;

namespace IncrementialMapper.Syntax.Kinds;

public enum PropertyTypeKind
{
    None = 0,
    /// <summary>
    /// See supported collections in <see cref="CollectionCasting.SupportedCollections"/>
    /// </summary>
    Collection = 1,
    Object = 2,
}