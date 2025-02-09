namespace AutoMorph.Abstractions.Enums;

public enum MappingStrategy
{
    /// <summary>
    /// Generates mapper from the source to the target.
    /// </summary>
    Normal = 1,
    /// <summary>
    /// Generates mapper from the target to the source, while being attached to the source.
    /// </summary>
    Reverse = 2,
    /// <summary>
    /// Generates mapper from the source to the target, and from the target to the source, while being attached to the source. 
    /// </summary>
    Both = Normal | Reverse
}