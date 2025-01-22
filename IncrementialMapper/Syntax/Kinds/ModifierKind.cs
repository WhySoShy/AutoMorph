namespace IncrementialMapper.Syntax.Kinds;

/// <summary>
/// Determines if the object should be created as static or partial.
/// </summary>
public enum ModifierKind
{
    Static = 0,
    Partial = 1,
    None = 3,
}