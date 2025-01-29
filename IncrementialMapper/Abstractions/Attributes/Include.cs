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

/// <summary>
/// This allows the use of generic mappers, pass an interface, class, struct or record to create the mapper with that restriction.
/// </summary>
public class Include<T>(MapperType type = MapperType.None, string? key = null) : Include(type, key);