using System;
using IncrementialMapper.Abstractions.Base;
using IncrementialMapper.Abstractions.Enums;

namespace IncrementialMapper.Abstractions.Attributes;

/// <summary>
/// The attribute works differently depending on what it is attached to. <br />
/// Read documentation, to see how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
public class Include(MapperType type = MapperType.None, string? key = null) : BaseAttribute(key);