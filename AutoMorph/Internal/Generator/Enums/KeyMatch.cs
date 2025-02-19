namespace AutoMorph.Internal.Generator.Enums;

internal enum KeyMatch
{
    /// <summary>
    /// The key present, should be applied to all properties.
    /// </summary>
    Global,
    
    /// <summary>
    /// The key and Method key does not math. 
    /// </summary>
    Invalid,
    
    /// <summary>
    /// The key and Method key does match.
    /// </summary>
    Valid
}