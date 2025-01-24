using System;
using System.Diagnostics;
using IncrementialMapper.Constants;

namespace IncrementialMapper.Attributes.Base;

/// <summary>
/// A base class that the attributes use to define a key.
/// </summary>
/// <param name="key">Used to define what mapper it should map to.</param>
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public abstract class BaseAttribute(string? key = null) : Attribute;