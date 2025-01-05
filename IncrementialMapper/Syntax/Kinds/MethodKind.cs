namespace IncrementialMapper.Syntax.Kinds;

internal enum MethodKind
{
    /// <summary>
    /// This is your standard object-object mapper.
    /// </summary>
    Standard,
    
    /// <summary>
    /// This is your standard list-list object mapper, it will map every object inside the collection of type <c>IEnumerable</c>.
    /// </summary>
    Linq,
    
    /// <summary>
    /// This adds support for the <c>IQueryable</c> collection, it will map every object inside the collection of type <c>IQueryable</c>.
    /// </summary>
    Linq_IQueryable,
}