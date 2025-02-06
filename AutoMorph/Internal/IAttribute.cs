namespace AutoMorph.Internal;

/// <summary>
/// A base class that the attributes use to define a key.
/// </summary>
internal interface IAttribute
{
    /// <summary>
    /// Defines what mapper the attribute should be targeted at. <br />
    /// If a key is present on the attribute, and the key exists on a <c>IncludeAttribute</c> then they will be 'attached' to each other. <br />
    /// If the key is not present on any attribute, then it will default to be a global attribute instead.
    /// </summary>
    public string? Key { get; set; }
}