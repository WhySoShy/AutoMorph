using System;
using AutoMorph.Abstractions.Enums;
using AutoMorph.Internal;
using AutoMorph.Internal.Constants;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// The attribute works differently depending on what it is attached to. <br />
/// Read documentation, to see how it works.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
public class IncludeAttribute(MapperType type = MapperType.None) : Attribute, IAttribute, IIncludeAttribute
{
    public string? Key { get; set; }

    public string? MapperName { get; set; }
}

/// <summary>
/// This allows the use of generic mappers, pass an interface, class, struct or record to create the mapper with that restriction.
/// </summary>
public class IncludeAttribute<T>(MapperType type = MapperType.None) : IncludeAttribute(type);

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IIncludeAttribute;