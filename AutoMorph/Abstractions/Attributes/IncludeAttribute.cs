using System;
using AutoMorph.Abstractions.Enums;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// This allows the use of generic mappers, pass an interface, class, struct or record to create the mapper with that restriction.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = true)]
public class IncludeAttribute<T>(MapperType type = MapperType.None) : Attribute, IIncludeAttribute
{
    public bool IsGeneric { get; set; }
}

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IIncludeAttribute;