namespace IncrementialMapper.Syntax.Kinds;


public enum PropertyKind
{
    None = 0,
    /// <summary>
    /// See supported collections in <see cref="CollectionCasting.SupportedCollections"/>
    /// </summary>
    Collection = 1,
    Object = 2,
}