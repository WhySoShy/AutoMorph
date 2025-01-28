namespace IncrementialMapper.Internal.Syntax.Types;

/// <summary>
/// The mapper type that should be generated.
/// </summary>
public enum MethodType
{
    /// <summary>
    /// This is your standard object-object mapper.
    /// </summary>
    Standard,
    
    /// <summary>
    /// This is your standard collection-collection object mapper, it will map every object inside the collection of type <c>IEnumerable</c>.
    /// </summary>
    Linq,
    
    /// <summary>
    /// This adds support for the <c>IQueryable</c> collection, it will map every object inside the collection of type <c>IQueryable</c>.
    /// </summary>
    IQueryable,
}