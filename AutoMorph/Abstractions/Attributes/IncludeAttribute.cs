using System;
using System.Diagnostics;
using AutoMorph.Abstractions.Enums;
using AutoMorph.Internal;
using AutoMorph.Internal.Constants;

namespace AutoMorph.Abstractions.Attributes;

/// <summary>
/// This allows the use of generic mappers, pass an interface, class, struct or record to create the mapper with that restriction.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
[Conditional(AssemblyConstants.EXCLUDED_CONDITIONAL_NAME)]
public class IncludeAttribute<T>(MapperType type, MappingStrategy strategy = MappingStrategy.Normal) : Attribute, IIncludeAttribute, IAttribute
{
    public string? MapperName { get; set; }
    public string? Key { get; set; }
    public bool IsGeneric { get; set; }
}

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IIncludeAttribute;