using System;

namespace IncrementialMapper.Attributes.Base;

/// <summary>
/// A base class that the attributes use to define a key.
/// </summary>
/// <param name="key">Used to define what mapper it should map to.</param>
public abstract class BaseAttribute(string? key = null) : Attribute;