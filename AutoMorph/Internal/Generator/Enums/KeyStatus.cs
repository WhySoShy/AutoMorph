namespace AutoMorph.Internal.Generator.Enums;

internal enum KeyStatus
{
    /// <summary>
    /// Meaning there is no key attached to the attribute.
    /// </summary>
    None,
    /// <summary>
    /// Meaning the key attached to the attribute was found on an Include.
    /// </summary>
    Valid,
    /// <summary>
    /// Meaning the key attached to the attribute was not found on any Include.
    /// </summary>
    Invalid,
}