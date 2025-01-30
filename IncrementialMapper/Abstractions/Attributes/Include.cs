using System;
using AutoMorph.Abstractions.Enums;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// The attribute works differently depending on what it is attached to. <br />
/// Read documentation, to see how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
public class Include(MapperType type = MapperType.None) : Attribute, IAttribute
{
    public string? Key { get; set; }
}

/// <summary>
/// This allows the use of generic mappers, pass an interface, class, struct or record to create the mapper with that restriction.
/// </summary>
public class Include<T>(MapperType type = MapperType.None) : Include(type);