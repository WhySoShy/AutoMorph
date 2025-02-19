namespace AutoMorph.Internal.Generator.Enums;

/// <summary>
/// Is either the Source or Target.
/// </summary>
internal enum Type
{
    /// <summary>
    /// This will check if the property contains an ExcludeProperty, with or without the MethodKey.
    /// </summary>
    Source,
    /// <summary>
    /// This will look for properties.
    /// </summary>
    Target
}