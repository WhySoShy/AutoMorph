using System;
using AutoMorph.Internal;

namespace AutoMorph.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class MapperAttribute : Attribute, IMapperAttribute
{
    /// <summary>
    /// The default name that will be applied to each generated mapper, if the mapper is not a partial mapper.  
    /// </summary>
    public string? DefaultMapperName { get; set; }
}

/// <summary>
/// Has no effect in the user code, only used to refer to the attribute in the generator.
/// </summary>
internal interface IMapperAttribute;