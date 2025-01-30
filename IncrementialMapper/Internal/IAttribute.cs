namespace IncrementialMapper.Internal;

/// <summary>
/// A base class that the attributes use to define a key.
/// </summary>
/// <param name="key">Used to define what mapper it should map to.</param>
internal interface IAttribute
{
    string? Key { get; }
}